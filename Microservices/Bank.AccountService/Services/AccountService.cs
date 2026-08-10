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
        var customerExistsResult = await _customerClient
            .CustomerExistsAsync(createAccountRequest.CustomerId!.Value);

        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateAccountResponse?>
                .Failure(Errors.CustomerClientError);
        }
        
        if(customerExistsResult.Data == false)
        {
            return ServiceResult<CreateAccountResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        
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

    public async Task<bool> DeleteAccountAsync(string accountNo)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
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
    
    public async Task<ServiceResult<bool>> CheckExistenceByAccountNoAsync(
        string accountNo)
    {
        bool exists = await _context.Accounts
            .AnyAsync(account => account.AccountNo == accountNo);

        return ServiceResult<bool>.Success(exists);
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

        decimal amount = request.Amount!.Value;

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

        var theAccount = await _context.Accounts
            .FirstOrDefaultAsync(
                account => account.AccountNo == request.AccountNo
            );

        if (theAccount is null)
        {
            return ServiceResult<DepositResponse>.Failure(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        var limitRequest = new UseChargeLimitRequest
        {
            Amount = amount,
            ChannelCode = request.ChannelCode!.Value,
            CustomerId = theAccount.CustomerId
        };

        bool limitUsed = false;
        bool depositMade = false;
        string? authorizationGuid = null;

        Guid operationId = Guid.NewGuid();
        
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
                await _context.Accounts
                    .Where(account => account.AccountNo == request.AccountNo)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(account => account.Balance,
                            account => account.Balance + request.Amount)
                    );

            if (affectedRows == 0)
            {
                throw new GeneralException(
                    Errors.AccountNotFoundError,
                    StatusCodes.Status404NotFound
                );
            }

            depositMade = true;

            await _context.Entry(theAccount)
                .ReloadAsync();

            var authorizationRequest = new CreateAuthorizationRequest
            {
                AccountNo = theAccount.AccountNo,
                Balance = theAccount.Balance,
                CardToken = null,
                ChannelCode = request.ChannelCode.Value,
                CustomerId = theAccount.CustomerId,
                Otc = Constants.Otcs.Deposit,
                Ots = request.ChannelCode == ChannelCode.Branch
                    ? Constants.Ots.DepositOts.BranchDeposit
                    : Constants.Ots.DepositOts.AtmDeposit,
                TransactionAmount = amount,
                TransactionDescription = "Deposit",
                TransactionStatus = "1",
                TransactionId = request.TransactionId
            };

            var createAuthorizationResult =
                await _authorizationClient.CreateAuthorizationAsync(
                    authorizationRequest
                );

            if (!createAuthorizationResult.IsSuccess)
            {
                throw new GeneralException(
                    createAuthorizationResult.Error
                    ?? Errors.AuthorizationClientError,
                    createAuthorizationResult.StatusCode
                );
            }

            if (createAuthorizationResult.Data is null)
            {
                throw new GeneralException(
                    Errors.AuthorizationServiceResponseError,
                    StatusCodes.Status502BadGateway
                );
            }

            CreateAuthorizationResponse authorization = createAuthorizationResult.Data;
            
            authorizationGuid = authorization.Guid;
            
            return ServiceResult<DepositResponse>.Success(
                new DepositResponse
                {
                    TransactionAmount = authorization.TransactionAmount,
                    Balance = authorization.Balance,
                    TransactionTime = authorization.TransactionDate,
                    TransactionId = request.TransactionId!.Value
                }
            );
        }
        catch (Exception)
        {

            var compensateLimitRequest = new CompensateUseChargeLimitRequest
            {
                OperationId = operationId,
                CustomerId = theAccount.CustomerId,
                Amount = amount
            };
            
            await CompensateDepositAsync(
                accountNo: request.AccountNo,
                amount: amount,
                limitRequest: compensateLimitRequest,
                authorizationGuid: authorizationGuid,
                depositMade: depositMade,
                limitUsed: limitUsed,
                operationId: operationId
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

        var theAccount = await _context.Accounts
            .FirstOrDefaultAsync(
                account => account.AccountNo == request.AccountNo
            );

        if (theAccount is null)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }
        
        var limitRequest = new UseChargeLimitRequest
        {
            Amount = amount,
            ChannelCode = request.ChannelCode!.Value,
            CustomerId = theAccount.CustomerId
        };
        

        bool withdrawalMade = false;
        string? authorizationGuid = null;
        bool limitUsed = false;
        
        Guid operationId = Guid.NewGuid();
        
        try
        {
            
            var limitResult = await _chargeLimitSaga.ExecuteAsync(
                limitRequest
            );

            if (!limitResult.IsSuccess)
            {
                return ServiceResult<WithdrawResponse>.Failure(
                    limitResult.Error ?? Errors.InsufficientLimitError,
                    limitResult.StatusCode
                );
            }

            limitUsed = true;
            
            int affectedRows =
                await _context.Accounts
                    .Where(account => account.AccountNo == request.AccountNo  && account.Balance >= request.Amount)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(account => account.Balance,
                            account => account.Balance - request.Amount)
                    );

            if (affectedRows == 0)
            {
                throw new GeneralException(
                    Errors.InsufficientFundsError,
                    StatusCodes.Status409Conflict
                );
            }

            withdrawalMade = true;

            // account was loaded before the SQL update,
            // so refresh its Balance from the database.
            await _context.Entry(theAccount).ReloadAsync();

            var authorizationRequest = new CreateAuthorizationRequest
            {
                AccountNo = theAccount.AccountNo,
                Balance = theAccount.Balance,
                CardToken = null,
                ChannelCode = request.ChannelCode,
                CustomerId = theAccount.CustomerId,
                Otc = Constants.Otcs.Withdrawal,
                Ots = Constants.Ots.WithdrawalOts.CashWithdrawal,
                TransactionAmount = amount,
                TransactionDescription = "Withdrawal",
                TransactionStatus = "1",
                TransactionId = request.TransactionId
            };

            var createAuthorizationResult =
                await _authorizationClient.CreateAuthorizationAsync(
                    authorizationRequest
                );


            if (!createAuthorizationResult.IsSuccess)
            {
                throw new GeneralException(
                    createAuthorizationResult.Error
                    ?? Errors.AuthorizationClientError,
                    createAuthorizationResult.StatusCode
                );
            }

            if (createAuthorizationResult.Data is null)
            {
                throw new GeneralException(
                    Errors.AuthorizationServiceResponseError,
                    StatusCodes.Status502BadGateway
                );
            }

            CreateAuthorizationResponse authorization = createAuthorizationResult.Data;

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
            
            var compensateLimitRequest = new CompensateUseChargeLimitRequest
            {
                OperationId = operationId,
                CustomerId = theAccount.CustomerId,
                Amount = amount
            };
            
            await CompensateWithdrawAsync(
                request.AccountNo,
                amount,
                compensateLimitRequest,
                authorizationGuid,
                withdrawalMade,
                limitUsed,
                operationId
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

        var theAccount = await _context.Accounts
            .FirstOrDefaultAsync(
                account => account.AccountNo == request.AccountNo
            );

        if (theAccount is null)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        var limitRequest = new UseChargeLimitRequest
        {
            Amount = amount,
            ChannelCode = request.ChannelCode!.Value,
            CustomerId = theAccount.CustomerId
        };
            

        bool withdrawalMade = false;
        string? authorizationGuid = null;
        bool limitUsed = false;
            
        Guid operationId = Guid.NewGuid();
        
        try
        {
                
            var limitResult = await _chargeLimitSaga.ExecuteAsync(
                limitRequest
            );

            if (!limitResult.IsSuccess)
            {
                return ServiceResult<WithdrawResponse>.Failure(
                    limitResult.Error ?? Errors.InsufficientLimitError,
                    limitResult.StatusCode
                );
            }

            limitUsed = true;
            
            int affectedRows =
                await _context.Accounts
                    .Where(account => account.AccountNo == request.AccountNo  && account.Balance >= request.Amount)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(account => account.Balance,
                            account => account.Balance - request.Amount)
                    );

            if (affectedRows == 0)
            {
                throw new GeneralException(
                    Errors.InsufficientFundsError,
                    StatusCodes.Status409Conflict
                );
            }

            withdrawalMade = true;

            // account was loaded before the raw SQL update,
            // so refresh its Balance from the database.
            await _context.Entry(theAccount).ReloadAsync();

            var authorizationRequest = new CreateAuthorizationRequest
            {
                AccountNo = theAccount.AccountNo,
                Balance = theAccount.Balance,
                CardToken = null,
                ChannelCode = request.ChannelCode,
                CustomerId = theAccount.CustomerId,
                Otc = Constants.Otcs.Withdrawal,
                Ots = Constants.Ots.WithdrawalOts.FastWihtdrawal,
                TransactionAmount = amount,
                TransactionDescription = "Fast Withdrawal",
                TransactionStatus = "1",
                TransactionId = request.TransactionId
            };

            var createAuthorizationResult =
                await _authorizationClient.CreateAuthorizationAsync(
                    authorizationRequest
                );

            if (!createAuthorizationResult.IsSuccess)
            {
                throw new GeneralException(
                    createAuthorizationResult.Error
                    ?? Errors.AuthorizationClientError,
                    createAuthorizationResult.StatusCode
                );
            }

            if (createAuthorizationResult.Data is null)
            {
                throw new GeneralException(
                    Errors.AuthorizationServiceResponseError,
                    StatusCodes.Status502BadGateway
                );
            }

            CreateAuthorizationResponse authorization = createAuthorizationResult.Data;

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
        catch (Exception)
        {

            var compensateLimitRequest = new CompensateUseChargeLimitRequest
            {
                OperationId = operationId,
                CustomerId = theAccount.CustomerId,
                Amount = amount
            };
            
            await CompensateWithdrawAsync(
                request.AccountNo,
                amount,
                compensateLimitRequest,
                authorizationGuid,
                withdrawalMade,
                limitUsed,
                operationId
            );

            throw;
        }
    }

    //should be called from authorization microservice
    public async Task<ServiceResult<SaleResponse>> SaleAsync(SaleRequest request)
    {
        int affectedRows =
            await _context.Accounts
                .Where(account => account.AccountNo == request.AccountNo && 
                                  account.Balance >= request.Amount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(account => account.Balance,
                        account => account.Balance - request.Amount)
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
    
    //should be called from authorization microservice
    public async Task<ServiceResult<RefundResponse>> RefundAsync(RefundRequest request)
    {
        int affectedRows =
            await _context.Accounts
                .Where(account => account.AccountNo == request.AccountNo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(account => account.Balance,
                        account => account.Balance + request.Amount)
                );

        if (affectedRows == 0)
        {
            return ServiceResult<RefundResponse>.Failure(Errors.AccountNotFoundError);
        }
        
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == request.AccountNo);

        if (account == null)
        {
            return ServiceResult<RefundResponse>.Failure(Errors.UnexpectedError);
        }
        
        RefundResponse response = new RefundResponse
        {
            TransactionId = request.TransactionId!.Value,
            Balance = account.Balance,
            CustomerId = account.CustomerId
        };
        return ServiceResult<RefundResponse>.Success(response);
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
    
    private async Task CompensateDepositBalanceAsync(
        Guid operationId,
        string accountNo,
        decimal amount)
    {
        const string operationType =
            Constants.CompensationOperationTypes
                .ReverseDepositBalance;

        bool alreadyCompleted =
            await _context.CompletedSagaOperations.AnyAsync(
                operation =>
                    operation.OperationId == operationId &&
                    operation.OperationType == operationType
            );

        if (alreadyCompleted)
        {
            return;
        }

        int affectedRows = await _context.Accounts
            .Where(account => account.AccountNo == accountNo)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    account => account.Balance,
                    account => account.Balance - amount
                )
            );

        if (affectedRows == 0)
        {
            throw new GeneralException(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        _context.CompletedSagaOperations.Add(
            new CompletedSagaOperation
            {
                OperationId = operationId,
                OperationType = operationType,
                CompletedAt = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
    }
    
    private async Task CompensateWithdrawBalanceAsync(
        Guid operationId,
        string accountNo,
        decimal amount)
    {
        const string operationType =
            Constants.CompensationOperationTypes
                .RestoreWithdrawBalance;

        bool alreadyCompleted =
            await _context.CompletedSagaOperations.AnyAsync(
                operation =>
                    operation.OperationId == operationId &&
                    operation.OperationType == operationType
            );

        if (alreadyCompleted)
        {
            return;
        }

        int affectedRows = await _context.Accounts
            .Where(account => account.AccountNo == accountNo)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    account => account.Balance,
                    account => account.Balance + amount
                )
            );

        if (affectedRows == 0)
        {
            throw new GeneralException(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        _context.CompletedSagaOperations.Add(
            new CompletedSagaOperation
            {
                OperationId = operationId,
                OperationType = operationType,
                CompletedAt = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
    }
    
    private async Task CompensateDepositAsync(
    Guid operationId,
    string accountNo,
    decimal amount,
    CompensateUseChargeLimitRequest limitRequest,
    string? authorizationGuid,
    bool depositMade,
    bool limitUsed)
    {

        if (authorizationGuid is not null)
        {
            try
            {
                var authorizationResult =
                    await _authorizationClient.AssignStatusAsync(
                        authorizationGuid,
                        "0"
                    );

                if (!authorizationResult.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. " +
                        "OperationId: {OperationId}, Guid: {Guid}",
                        operationId,
                        authorizationGuid
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. " +
                    "OperationId: {OperationId}, Guid: {Guid}",
                    operationId,
                    authorizationGuid
                );
            }
        }

        if (!depositMade && !limitUsed)
        {
            return;
        }

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Reverse order of the original local operations.

            if (depositMade)
            {
                await CompensateDepositBalanceAsync(
                    operationId,
                    accountNo,
                    amount
                );
            }

            if (limitUsed)
            {
                var limitResult =
                    await _chargeLimitSaga.CompensateAsync(
                        limitRequest
                    );

                if (!limitResult.IsSuccess)
                {
                    throw new GeneralException(
                        limitResult.Error
                            ?? Errors.ChargeLimitCompensateError,
                        limitResult.StatusCode
                    );
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Local deposit compensation failed. " +
                "OperationId: {OperationId}, AccountNo: {AccountNo}",
                operationId,
                accountNo
            );
        }
    }
    
    private async Task CompensateWithdrawAsync(
        string accountNo,
        decimal amount,
        CompensateUseChargeLimitRequest limitRequest,
        string? authorizationGuid,
        bool withdrawMade,
        bool limitUsed,
        Guid operationId
        )
    {

        if (authorizationGuid is not null)
        {
            try
            {
                var authorizationResult =
                    await _authorizationClient.AssignStatusAsync(
                        authorizationGuid,
                        "0"
                    );

                if (!authorizationResult.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. " +
                        "OperationId: {OperationId}, Guid: {Guid}",
                        operationId,
                        authorizationGuid
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. " +
                    "OperationId: {OperationId}, Guid: {Guid}",
                    operationId,
                    authorizationGuid
                );
            }
        }

        if (!withdrawMade && !limitUsed)
        {
            return;
        }

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            // Reverse order of the original local operations.

            if (withdrawMade)
            {
                await CompensateWithdrawBalanceAsync(
                    operationId,
                    accountNo,
                    amount
                );
            }

            if (limitUsed)
            {
                var limitResult =
                    await _chargeLimitSaga.CompensateAsync(
                        limitRequest
                    );

                if (!limitResult.IsSuccess)
                {
                    throw new GeneralException(
                        limitResult.Error
                            ?? Errors.ChargeLimitCompensateError,
                        limitResult.StatusCode
                    );
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Withdrawal compensation failed. " +
                "OperationId: {OperationId}, AccountNo: {AccountNo}",
                operationId,
                accountNo
            );
        }
    }

    public async Task<ServiceResult<SaleResponse>>
        CompensateSaleAsync(CompensateSaleRequest request)
    {
        const string operationType =
            Constants.CompensationOperationTypes.RestoreSaleBalance;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        bool alreadyCompleted =
            await _context.CompletedSagaOperations.AnyAsync(operation =>
                operation.OperationId == request.OperationId &&
                operation.OperationType == operationType
            );

        if (alreadyCompleted)
        {
            var existingAccount = await _context.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(account =>
                    account.AccountNo == request.AccountNo);

            if (existingAccount is null)
            {
                return ServiceResult<SaleResponse>.Failure(
                    Errors.AccountNotFoundError,
                    StatusCodes.Status404NotFound
                );
            }

            return ServiceResult<SaleResponse>.Success(
                new SaleResponse
                {
                    Balance = existingAccount.Balance,
                    CustomerId = existingAccount.CustomerId
                }
            );
        }

        int affectedRows = await _context.Accounts
            .Where(account => account.AccountNo == request.AccountNo)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    account => account.Balance,
                    account => account.Balance + request.Amount
                )
            );

        if (affectedRows == 0)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        _context.CompletedSagaOperations.Add(
            new CompletedSagaOperation
            {
                OperationId = request.OperationId.Value,
                OperationType = operationType,
                CompletedAt = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var account = await _context.Accounts
            .AsNoTracking()
            .SingleAsync(account =>
                account.AccountNo == request.AccountNo);

        return ServiceResult<SaleResponse>.Success(
            new SaleResponse
            {
                Balance = account.Balance,
                CustomerId = account.CustomerId
            }
        );
    }
    
    public async Task<ServiceResult<RefundResponse>>
        CompensateRefundAsync(CompensateRefundRequest request)
    {
        const string operationType =
            Constants.CompensationOperationTypes.RestoreRefundBalance;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        bool alreadyCompleted =
            await _context.CompletedSagaOperations.AnyAsync(operation =>
                operation.OperationId == request.OperationId &&
                operation.OperationType == operationType
            );

        if (alreadyCompleted)
        {
            var existingAccount = await _context.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(account =>
                    account.AccountNo == request.AccountNo);

            if (existingAccount is null)
            {
                return ServiceResult<RefundResponse>.Failure(
                    Errors.AccountNotFoundError,
                    StatusCodes.Status404NotFound
                );
            }

            return ServiceResult<RefundResponse>.Success(
                new RefundResponse
                {
                    Balance = existingAccount.Balance,
                    CustomerId = existingAccount.CustomerId
                }
            );
        }

        int affectedRows = await _context.Accounts
            .Where(account => account.AccountNo == request.AccountNo)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    account => account.Balance,
                    account => account.Balance - request.Amount
                )
            );

        if (affectedRows == 0)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        _context.CompletedSagaOperations.Add(
            new CompletedSagaOperation
            {
                OperationId = request.OperationId.Value,
                OperationType = operationType,
                CompletedAt = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var account = await _context.Accounts
            .AsNoTracking()
            .SingleAsync(account =>
                account.AccountNo == request.AccountNo);

        return ServiceResult<RefundResponse>.Success(
            new RefundResponse
            {
                Balance = account.Balance,
                CustomerId = account.CustomerId
            }
        );
    }
}