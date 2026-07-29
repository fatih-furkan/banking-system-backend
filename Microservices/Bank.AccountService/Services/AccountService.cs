using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.AccountService.Models.Dtos;
using Bank.AccountService.Models.Entities;
using Bank.Shared.Constants;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Services;

public class AccountService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;
    private readonly AuthorizationClient _authorizationClient;

    public AccountService(AppDbContext context, 
        CustomerClient customerClient,
        AuthorizationClient authorizationClient)
    {
        _context = context;
        _customerClient = customerClient;
        _authorizationClient = authorizationClient;
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
        bool customerExists = await _customerClient.CustomerExistsAsync(createAccountRequest.CustomerId.Value);
        
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

    public async Task<ServiceResult<DepositResponse>> DepositAsync(DepositRequest depositRequest)
    {

        if (depositRequest.ChannelCode == ChannelCode.Pos)
        {
            return ServiceResult<DepositResponse>.Failure(
                Errors.UnauthorizedChannelError, 403);
        }
        if (decimal.Round(depositRequest.Amount.Value, 2) != depositRequest.Amount)
        {
            return ServiceResult<DepositResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (depositRequest.Amount < 0)
        {
            return ServiceResult<DepositResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE ACCOUNT
             SET BALANCE = BALANCE + {depositRequest.Amount}
             WHERE ACCOUNT_NO = {depositRequest.AccountNo}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<DepositResponse>.Failure(Errors.AccountNotFoundError);
        }
        
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == depositRequest.AccountNo);

        if (account != null)
        {
            var authRequest = new CreateAuthorizationRequest
            {
                AccountNo = account.AccountNo,
                Balance = account.Balance,
                CardToken = null,
                ChannelCode = depositRequest.ChannelCode.Value,
                CustomerId = account.CustomerId,
                Otc = Constants.Otcs.Deposit,
                Ots = depositRequest.ChannelCode == ChannelCode.Branch 
                    ? Constants.Ots.DepositOts.BranchDeposit : Constants.Ots.DepositOts.AtmDeposit,
                TransactionAmount = depositRequest.Amount,
                TransactionDescription = "deposit",
                TransactionStatus = "1"
            };

            var authResponse = await _authorizationClient.CreateAuthorizationAsync(authRequest);
            
            DepositResponse response = new DepositResponse
            {
                TransactionAmount = authResponse.TransactionAmount,
                Balance = authResponse.Balance,
                TransactionTime = authResponse.TransactionDate
            };
            return ServiceResult<DepositResponse>.Success(response);
        }
        else return ServiceResult<DepositResponse>.Failure(Errors.AccountNotFoundError);
    }
    
    public async Task<ServiceResult<WithdrawResponse>> CashWithdrawAsync(WithdrawRequest withdrawRequest)
    {
        
        if (decimal.Round(withdrawRequest.Amount.Value, 2) != withdrawRequest.Amount)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.PrecisionError, 403);
        }
        
        if (withdrawRequest.Amount < 0)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE ACCOUNT
             SET BALANCE = BALANCE - {withdrawRequest.Amount}
             WHERE ACCOUNT_NO = {withdrawRequest.AccountNo} AND BALANCE >= {withdrawRequest.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<WithdrawResponse>.Failure(Errors.InsufficientFundsError);
        }
        
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == withdrawRequest.AccountNo);

        if (account != null)
        {
            
            var authRequest = new CreateAuthorizationRequest
            {
                AccountNo = account.AccountNo,
                Balance = account.Balance,
                CardToken = null,
                ChannelCode = withdrawRequest.ChannelCode,
                CustomerId = account.CustomerId,
                Otc = Constants.Otcs.Withdrawal,
                Ots = Constants.Ots.WithdrawalOts.CashWithdrawal,
                TransactionAmount = withdrawRequest.Amount,
                TransactionDescription = "withdrawal",
                TransactionStatus = "1"
            };

            var authResponse = await _authorizationClient.CreateAuthorizationAsync(authRequest);
            WithdrawResponse response = new WithdrawResponse
            {
                TransactionAmount = authResponse.TransactionAmount,
                Balance = authResponse.Balance,
                TransactionTime = authResponse.TransactionDate
            };
            return ServiceResult<WithdrawResponse>.Success(response);
        }
        else return ServiceResult<WithdrawResponse>.Failure(Errors.AccountNotFoundError);
    }
    
        public async Task<ServiceResult<WithdrawResponse>> FastWithdrawAsync(WithdrawRequest withdrawRequest)
    {
        
        if (decimal.Round(withdrawRequest.Amount.Value, 2) != withdrawRequest.Amount)
        {
            return ServiceResult<WithdrawResponse>.Failure(
                Errors.PrecisionError, 403);
        }
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE ACCOUNT
             SET BALANCE = BALANCE - {withdrawRequest.Amount}
             WHERE ACCOUNT_NO = {withdrawRequest.AccountNo} AND BALANCE >= {withdrawRequest.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<WithdrawResponse>.Failure(Errors.InsufficientFundsError);
        }
        
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == withdrawRequest.AccountNo);

        if (account != null)
        {
            
            var authRequest = new CreateAuthorizationRequest
            {
                AccountNo = account.AccountNo,
                Balance = account.Balance,
                CardToken = null,
                ChannelCode = withdrawRequest.ChannelCode,
                CustomerId = account.CustomerId,
                Otc = Constants.Otcs.Withdrawal,
                Ots = Constants.Ots.WithdrawalOts.FastWihtdrawal,
                TransactionAmount = withdrawRequest.Amount,
                TransactionDescription = "fast withdrawal",
                TransactionStatus = "1"
            };

            var authResponse = await _authorizationClient.CreateAuthorizationAsync(authRequest);
            WithdrawResponse response = new WithdrawResponse
            {
                TransactionAmount = authResponse.TransactionAmount,
                Balance = authResponse.Balance,
                TransactionTime = authResponse.TransactionDate
            };
            return ServiceResult<WithdrawResponse>.Success(response);
        }
        else return ServiceResult<WithdrawResponse>.Failure(Errors.InsufficientFundsError);
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
    
    public async Task<bool> CheckExistenceByAccountNoAsync(string accountNo)
    {
        return await _context.Accounts.AnyAsync(account => account.AccountNo == accountNo);
    }
}