using Bank.PointService.Clients;
using Bank.PointService.Data;
using Bank.PointService.Models.Dtos;
using Bank.PointService.Models.Entities;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.PointService.Services;

public class PointService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;

    public PointService(AppDbContext context,
        CustomerClient customerClient)
    {
        _context = context;
        _customerClient = customerClient;
    }
    
    public async Task<List<PointAccount>> GetAllPointAccountsAsync()
    {
        return await _context.PointAccounts.ToListAsync();
    }
    
    public async Task<PointAccount?> GetPointAccountByAccountNoAsync(string accountNo)
    {
        return await _context.PointAccounts.FindAsync(accountNo);
    }
    
    public async Task<ServiceResult<CreatePointAccountResponse?>> AddPointAccountAsync(
        CreatePointAccountRequest request)
    {
        var customerExistsResult = await _customerClient
            .CustomerExistsAsync(request.CustomerId!.Value);

        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreatePointAccountResponse?>
                .Failure(Errors.CustomerClientError);
        }
        
        if(customerExistsResult.Data == false)
        {
            return ServiceResult<CreatePointAccountResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        
        var accountNo = await GetNextPointAccountNoSequenceValueAsync();
        var pointAccount = new PointAccount
        {
            AccountNo = accountNo.ToString(),
            CustomerId = request.CustomerId.Value,
            Status = "1",
            EarnedPoint = 0,
            UsedPoint = 0,
            ExpiredPoint = 0
        };
        
        _context.PointAccounts.Add(pointAccount);
        await _context.SaveChangesAsync();
        CreatePointAccountResponse response = new CreatePointAccountResponse
        {
            AccountNo = pointAccount.AccountNo,
            CustomerId = pointAccount.CustomerId,
            Status = pointAccount.Status,
            UsedPoint = pointAccount.UsedPoint,
            EarnedPoint = pointAccount.EarnedPoint,
            ExpiredPoint = pointAccount.ExpiredPoint
        };
        return ServiceResult<CreatePointAccountResponse?>.Success(response);
    }
    
    public async Task<bool> DeletePointAccountAsync(string accountNo)
    {
        var account = await _context.PointAccounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        if (account != null)
        {
            _context.PointAccounts.Remove(account);
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
        
        var account = await _context.PointAccounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        
        if (account == null)
        {
            return ServiceResult<Unit>.Failure(Errors.PointAccountNotFoundError);
        }
        
        account.Status = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }

    public async Task<ServiceResult<Unit>> CompensateCreatePointAccountAsync(string pointAccountNo)
    {
        await _context.PointAccounts
            .Where(x => x.AccountNo == pointAccountNo)
            .ExecuteDeleteAsync();
        
        return ServiceResult<Unit>.Success(new Unit());
    }
    
    private async Task<long> GetNextPointAccountNoSequenceValueAsync()
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT POINT_ACC_NO_SEQ.NEXTVAL FROM DUAL";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }
    
}