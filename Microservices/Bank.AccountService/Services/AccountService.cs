using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.AccountService.Models.Dtos.Account;
using Bank.AccountService.Models.Dtos.Limit;
using Bank.AccountService.Models.Entities.Account;
using Bank.AccountService.Sagas;
using Bank.Shared.Constants;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Services;

public class AccountService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;
    private readonly AuthorizationClient _authorizationClient;
    private readonly ChargeLimitSaga _chargeLimitSaga;
    private readonly ILogger<AccountService> _logger;
    
    public AccountService(AppDbContext context, 
        CustomerClient customerClient,
        AuthorizationClient authorizationClient,
        ChargeLimitSaga chargeLimitSaga,
        ILogger<AccountService> logger)
    {
        _context = context;
        _customerClient = customerClient;
        _authorizationClient = authorizationClient;
        _chargeLimitSaga = chargeLimitSaga;
        _logger = logger;
    }
     
    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetAccountByAccountNoAsync(string accountNo)
    {
        return await _context.Accounts.FindAsync(accountNo);
    }

    public async Task<ServiceResult<CreateAccountResponse?>> AddAccountAsync(
        CreateAccountRequest createAccountRequest)
    {
        bool customerExists = await _customerClient.CustomerExistsAsync(createAccountRequest.CustomerId!.Value);
        
        if(!customerExists)
        {
            return ServiceResult<CreateAccountResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        else
        {
            var accountNo = await GetNextAccountNoSequenceValueAsync();
            var account = new Account
            {
                AccountNo = accountNo.ToString(),
                BranchCode = createAccountRequest.BranchCode,
                CustomerId = createAccountRequest.CustomerId.Value,
                Status = createAccountRequest.Status,
                Balance = 0
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            CreateAccountResponse response = new CreateAccountResponse
            {
                AccountNo = account.AccountNo,
                BranchCode = account.BranchCode,
                CustomerId = account.CustomerId,
                Status = account.Status
            };
            return ServiceResult<CreateAccountResponse?>.Success(response);
        }
    }

    public async Task<bool> DeleteAccountAsync(string accountNo)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
        public async Task<ServiceResult<DepositResponse>> DepositAsync(
    DepositRequest request)
{
    if (request.ChannelCode == ChannelCode.Pos)
    {
        return ServiceResult<DepositResponse>.Failure(
            Errors.UnauthorizedChannelError,
            StatusCodes.Status403Forbidden
        );
    }

    decimal amount = request.Amount.Value;

    if (decimal.Round(amount, 2) != amount)
    {
        return ServiceResult<DepositResponse>.Failure(
            Errors.PrecisionError,
            StatusCodes.Status400BadRequest
        );
    }

    if (amount <= 0)
    {
        return ServiceResult<DepositResponse>.Failure(
            Errors.NegativeAmountError,
            StatusCodes.Status400BadRequest
        );
    }

    var account = await _context.Accounts
        .FirstOrDefaultAsync(
            account => account.AccountNo == request.AccountNo
        );

    if (account is null)
    {
        return ServiceResult<DepositResponse>.Failure(
            Errors.AccountNotFoundError,
            StatusCodes.Status404NotFound
        );
    }

    var limitRequest = new UseChargeLimitRequest
    {
        Amount = amount,
        ChannelCode = request.ChannelCode.Value,
        CustomerId = account.CustomerId
    };

    bool limitUsed = false;
    bool depositMade = false;
    string? authorizationGuid = null;

    try
    {
        var limitResult = await _chargeLimitSaga.ExecuteAsync(
            limitRequest
        );

        if (!limitResult.IsSuccess)
        {
            return ServiceResult<DepositResponse>.Failure(
                limitResult.Error ?? Errors.InsufficientLimitError,
                limitResult.StatusCode
            );
        }

        limitUsed = true;
        
        int affectedRows =
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE ACCOUNT
                SET BALANCE = BALANCE + {amount}
                WHERE ACCOUNT_NO = {request.AccountNo}
                """
            );

        if (affectedRows == 0)
        {
            throw new GeneralException(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        depositMade = true;

        await _context.Entry(account)
            .ReloadAsync();

        var authorizationRequest = new CreateAuthorizationRequest
        {
            AccountNo = account.AccountNo,
            Balance = account.Balance,
            CardToken = null,
            ChannelCode = request.ChannelCode.Value,
            CustomerId = account.CustomerId,
            Otc = Constants.Otcs.Deposit,
            Ots = request.ChannelCode == ChannelCode.Branch
                ? Constants.Ots.DepositOts.BranchDeposit
                : Constants.Ots.DepositOts.AtmDeposit,
            TransactionAmount = amount,
            TransactionDescription = "Deposit",
            TransactionStatus = "1",
            TransactionId = request.TransactionId
        };

        var authorization =
            await _authorizationClient.CreateAuthorizationAsync(
                authorizationRequest
            );

        authorizationGuid = authorization.Guid;
        
        return ServiceResult<DepositResponse>.Success(
            new DepositResponse
            {
                TransactionAmount = authorization.TransactionAmount,
                Balance = authorization.Balance,
                TransactionTime = authorization.TransactionDate
            }
        );
    }
    catch (Exception exception)
    {
        _logger.LogError(
            exception,
            "Deposit failed. Starting compensation. AccountNo: {AccountNo}",
            request.AccountNo
        );

        await CompensateDepositAsync(
            request.AccountNo,
            amount,
            limitRequest,
            authorizationGuid,
            depositMade,
            limitUsed
        );

        throw;
    }
}

    public async Task<ServiceResult<WithdrawResponse>> CashWithdrawAsync(
    WithdrawRequest request)
{
    decimal amount = request.Amount!.Value;

    if (decimal.Round(amount, 2) != amount)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.PrecisionError,
            StatusCodes.Status400BadRequest
        );
    }

    if (amount <= 0)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.NegativeAmountError,
            StatusCodes.Status400BadRequest
        );
    }

    var account = await _context.Accounts
        .FirstOrDefaultAsync(
            account => account.AccountNo == request.AccountNo
        );

    if (account is null)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.AccountNotFoundError,
            StatusCodes.Status404NotFound
        );
    }

    bool withdrawalMade = false;
    string? authorizationGuid = null;

    try
    {
        int affectedRows =
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE ACCOUNT
                SET BALANCE = BALANCE - {amount}
                WHERE ACCOUNT_NO = {request.AccountNo}
                  AND BALANCE >= {amount}
                """
            );

        if (affectedRows == 0)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.InsufficientFundsError
            );
        }

        withdrawalMade = true;

        // account was loaded before the raw SQL update,
        // so refresh its Balance from the database.
        await _context.Entry(account).ReloadAsync();

        var authorizationRequest = new CreateAuthorizationRequest
        {
            AccountNo = account.AccountNo,
            Balance = account.Balance,
            CardToken = null,
            ChannelCode = request.ChannelCode,
            CustomerId = account.CustomerId,
            Otc = Constants.Otcs.Withdrawal,
            Ots = Constants.Ots.WithdrawalOts.CashWithdrawal,
            TransactionAmount = amount,
            TransactionDescription = "Withdrawal",
            TransactionStatus = "1",
            TransactionId = request.TransactionId
        };

        var authorization =
            await _authorizationClient.CreateAuthorizationAsync(
                authorizationRequest
            );

        authorizationGuid = authorization.Guid;

        return ServiceResult<WithdrawResponse>.Success(
            new WithdrawResponse
            {
                TransactionAmount = authorization.TransactionAmount,
                Balance = authorization.Balance,
                TransactionTime = authorization.TransactionDate,
                TransactionId = request.TransactionId!.Value
            }
        );
    }
    catch (Exception exception)
    {
        _logger.LogError(
            exception,
            "Cash withdrawal failed. Starting compensation. " +
            "AccountNo: {AccountNo}. TransactionId: {TransactionId}",
            request.AccountNo,
            request.TransactionId
        );

        await CompensateWithdrawAsync(
            request.AccountNo,
            amount,
            authorizationGuid,
            withdrawalMade
        );

        throw;
    }
}
    
    public async Task<ServiceResult<WithdrawResponse>> FastWithdrawAsync(WithdrawRequest request)
    {
        
    decimal amount = request.Amount!.Value;

    if (decimal.Round(amount, 2) != amount)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.PrecisionError,
            StatusCodes.Status400BadRequest
        );
    }

    if (amount <= 0)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.NegativeAmountError,
            StatusCodes.Status400BadRequest
        );
    }

    var account = await _context.Accounts
        .FirstOrDefaultAsync(
            account => account.AccountNo == request.AccountNo
        );

    if (account is null)
    {
        return ServiceResult<WithdrawResponse>.Failure(
            Errors.AccountNotFoundError,
            StatusCodes.Status404NotFound
        );
    }

    bool withdrawalMade = false;
    string? authorizationGuid = null;

    try
    {
        int affectedRows =
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE ACCOUNT
                SET BALANCE = BALANCE - {amount}
                WHERE ACCOUNT_NO = {request.AccountNo}
                  AND BALANCE >= {amount}
                """
            );

        if (affectedRows == 0)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.InsufficientFundsError
            );
        }

        withdrawalMade = true;

        // account was loaded before the raw SQL update,
        // so refresh its Balance from the database.
        await _context.Entry(account).ReloadAsync();

        var authorizationRequest = new CreateAuthorizationRequest
        {
            AccountNo = account.AccountNo,
            Balance = account.Balance,
            CardToken = null,
            ChannelCode = request.ChannelCode,
            CustomerId = account.CustomerId,
            Otc = Constants.Otcs.Withdrawal,
            Ots = Constants.Ots.WithdrawalOts.FastWihtdrawal,
            TransactionAmount = amount,
            TransactionDescription = "Fast Withdrawal",
            TransactionStatus = "1",
            TransactionId = request.TransactionId
        };

        var authorization =
            await _authorizationClient.CreateAuthorizationAsync(
                authorizationRequest
            );

        authorizationGuid = authorization.Guid;

        return ServiceResult<WithdrawResponse>.Success(
            new WithdrawResponse
            {
                TransactionAmount = authorization.TransactionAmount,
                Balance = authorization.Balance,
                TransactionTime = authorization.TransactionDate,
                TransactionId = request.TransactionId!.Value
            }
        );
    }
    catch (Exception exception)
    {
        _logger.LogError(
            exception,
            "Fast withdrawal failed. Starting compensation. " +
            "AccountNo: {AccountNo}. TransactionId: {TransactionId}",
            request.AccountNo,
            request.TransactionId
        );

        await CompensateWithdrawAsync(
            request.AccountNo,
            amount,
            authorizationGuid,
            withdrawalMade
        );

        throw;
    }
    }

    public async Task<ServiceResult<Unit>> AssignStatusAsync(AssignStatusRequest request, string accountNo)
    {
        bool isOnlyDigits =
            !string.IsNullOrEmpty(request.Status) &&
            request.Status.All(c => c is >= '0' and <= '9');
        
        if (!isOnlyDigits)
        {
            return ServiceResult<Unit>.Failure(Errors.InvalidStatusError);
        }
        
        var account = await _context.Accounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        
        if (account == null)
        {
            return ServiceResult<Unit>.Failure(Errors.AccountNotFoundError);
        }
        
        account.Status = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }
    
    public async Task<bool> CheckExistenceByAccountNoAsync(string accountNo)
    {
        return await _context.Accounts.AnyAsync(account => account.AccountNo == accountNo);
    }

    //should be called from authorization
    public async Task<ServiceResult<SaleResponse>> SaleAsync(SaleRequest request)
    {
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE ACCOUNT
             SET BALANCE = BALANCE - {request.Amount}
             WHERE ACCOUNT_NO = {request.AccountNo} AND BALANCE >= {request.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<SaleResponse>.Failure(Errors.InsufficientFundsError);
        }
        
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == request.AccountNo);

        if (account == null)
        {
            return ServiceResult<SaleResponse>.Failure(Errors.UnexpectedError);
        }
        
        SaleResponse response = new SaleResponse
        {
            TransactionId = request.TransactionId!.Value,
            Balance = account.Balance,
            CustomerId = account.CustomerId
        };
        return ServiceResult<SaleResponse>.Success(response);
    }
    
    private async Task<long> GetNextAccountNoSequenceValueAsync()
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ACCOUNT_NO_SEQ.NEXTVAL FROM DUAL";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }
    
    private async Task CompensateDepositAsync(
        string accountNo,
        decimal amount,
        UseChargeLimitRequest limitRequest,
        string? authorizationGuid,
        bool depositMade,
        bool limitUsed)
    {
        // Reverse order of completed operations.

        if (authorizationGuid is not null)
        {
            try
            {
                var result = await _authorizationClient.AssignStatusAsync(
                    authorizationGuid,
                    "0"
                );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. Guid: {Guid}",
                        authorizationGuid
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. Guid: {Guid}",
                    authorizationGuid
                );
            }
        }

        if (depositMade)
        {
            try
            {
                int affectedRows =
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"""
                        UPDATE ACCOUNT
                        SET BALANCE = BALANCE - {amount}
                        WHERE ACCOUNT_NO = {accountNo}
                        """,
                        CancellationToken.None
                    );

                if (affectedRows == 0)
                {
                    _logger.LogError(
                        "Deposit compensation failed. AccountNo: {AccountNo}",
                        accountNo
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Deposit compensation threw an exception. AccountNo: {AccountNo}",
                    accountNo
                );
            }
        }

        if (limitUsed)
        {
            try
            {
                await _chargeLimitSaga.CompensateAsync(
                    limitRequest
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Limit compensation failed. CustomerId: {CustomerId}",
                    limitRequest.CustomerId
                );
            }
        }
    }
    
    private async Task CompensateWithdrawAsync(
        string accountNo,
        decimal amount,
        string? authorizationGuid,
        bool depositMade
        )
    {
        // Reverse order of completed operations.

        if (authorizationGuid is not null)
        {
            try
            {
                var result = await _authorizationClient.AssignStatusAsync(
                    authorizationGuid,
                    "0"
                );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. Guid: {Guid}",
                        authorizationGuid
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. Guid: {Guid}",
                    authorizationGuid
                );
            }
        }

        if (depositMade)
        {
            try
            {
                int affectedRows =
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"""
                        UPDATE ACCOUNT
                        SET BALANCE = BALANCE + {amount}
                        WHERE ACCOUNT_NO = {accountNo}
                        """,
                        CancellationToken.None
                    );

                if (affectedRows == 0)
                {
                    _logger.LogError(
                        "Deposit compensation failed. AccountNo: {AccountNo}",
                        accountNo
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Deposit compensation threw an exception. AccountNo: {AccountNo}",
                    accountNo
                );
            }
        }
    }
}