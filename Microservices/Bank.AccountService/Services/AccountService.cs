using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Models.ClientModels;
using Bank.Shared;
using Bank.AccountService.Models.Dtos;
using Bank.AccountService.Models.Entities;
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
        bool customerExists = await _customerClient.CustomerExistsAsync(createAccountRequest.CustomerId);
        
        if(!customerExists)
        {
            return ServiceResult<CreateAccountResponse?>
                .Failure("The user does not exist.");
        }
        else
        {
            var accountNo = await GetNextAccountNoSequenceValueAsync();
            var account = new Account
            {
                AccountNo = accountNo.ToString(),
                BranchCode = createAccountRequest.BranchCode,
                CustomerId = createAccountRequest.CustomerId,
                Status = createAccountRequest.Status,
                Balance = 0
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            CreateAccountResponse response = new CreateAccountResponse
            {
                AccountNo = account.AccountNo,
                BranchCode = account.BranchCode,
                CardToken = account.CardToken,
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
        var account = await _context.Accounts.FirstOrDefaultAsync
            (account => account.AccountNo == depositRequest.AccountNo);

        if (account != null)
        {
            account.Balance += depositRequest.Amount;
            await _context.SaveChangesAsync();
            var authRequest = new CreateAuthorizationRequest
            {
                AccountNo = account.AccountNo,
                Balance = account.Balance, //eskisi mi konmali yenisi mi??
                CardToken = null,
                ChannelCode = depositRequest.ChannelCode,
                CustomerId = account.CustomerId,
                Otc = 10,
                Ots = 10,
                TransactionAmount = depositRequest.Amount,
                TransactionDescription = "desc", //todo Disaridan mi alinmali burada mi belirlenmeli??
                TransactionStatus = "1", //todo bu statuler tam neleri ifade ediyor??
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
        else return ServiceResult<DepositResponse>.Failure("Account does not exist.");
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
    
}