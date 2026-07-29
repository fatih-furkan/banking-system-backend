using Bank.LimitService.Clients;
using Bank.LimitService.Data;
using Bank.LimitService.Models.Dtos;
using Bank.LimitService.Models.Entities;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.LimitService.Services;

public class LimitService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;

    public LimitService(AppDbContext appDbContext, CustomerClient customerClient)
    {
        _context = appDbContext;
        _customerClient = customerClient;
    }
    
    public async Task<List<ChargeLimit>> GetAllChargeLimitsAsync()
    {
        return await _context.ChargeLimits.ToListAsync();
    }
    
    public async Task<ChargeLimit?> GetChargeLimitByCustomerIdAsync(long customerId)
    {
        return await _context.ChargeLimits.FindAsync(customerId);
    }
    
    public async Task<List<CurrentChargeLimit>> GetAllCurrentChargeLimitsAsync()
    {
        return await _context.CurrentChargeLimits.ToListAsync();
    }
    
    public async Task<CurrentChargeLimit?> GetCurrentChargeLimitByCustomerIdAsync(long customerId)
    {
        return await _context.CurrentChargeLimits.FindAsync(customerId);
    }
    
    public async Task<ServiceResult<CreateChargeLimitResponse?>> AddChargeLimitAsync(
        CreateChargeLimitRequest createChargeLimitRequest)
    {
        bool customerExists = await _customerClient.CustomerExistsAsync(createChargeLimitRequest.CustomerId.Value);
        
        if(!customerExists)
        {
            return ServiceResult<CreateChargeLimitResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        else
        {
            var limit = new ChargeLimit
            {
                CustomerId = createChargeLimitRequest.CustomerId.Value,
                AnnualLimit = 0,
                DailyLimit = 0,
                MonthlyLimit = 0
                //todo limitler nasıl belirleniyor
            };
            
            _context.ChargeLimits.Add(limit);
            await _context.SaveChangesAsync();
            CreateChargeLimitResponse response = new CreateChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                AnnualLimit = limit.AnnualLimit,
                MonthlyLimit = limit.MonthlyLimit,
                DailyLimit = limit.MonthlyLimit
            };
            return ServiceResult<CreateChargeLimitResponse?>.Success(response);
        }
    }
    
    public async Task<ServiceResult<CreateCurrentChargeLimitResponse?>> AddCurrentChargeLimitAsync(
        CreateCurrentChargeLimitRequest createCurrentChargeLimitRequest)
    {
        bool customerExists = await _customerClient.CustomerExistsAsync(createCurrentChargeLimitRequest.CustomerId.Value);
        
        if(!customerExists)
        {
            return ServiceResult<CreateCurrentChargeLimitResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        else
        {
            var limit = new CurrentChargeLimit
            {
                CustomerId = createCurrentChargeLimitRequest.CustomerId.Value,
                AnnualLimit = 0,
                DailyLimit = 0,
                MonthlyLimit = 0
                //todo limitler nasıl belirleniyor
            };
            
            _context.CurrentChargeLimits.Add(limit);
            await _context.SaveChangesAsync();
            CreateCurrentChargeLimitResponse response = new CreateCurrentChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                AnnualLimit = limit.AnnualLimit,
                MonthlyLimit = limit.MonthlyLimit,
                DailyLimit = limit.MonthlyLimit
            };
            return ServiceResult<CreateCurrentChargeLimitResponse?>.Success(response);
        }
    }

    public async Task<bool> DeleteChargeLimitAsync(long customerId)
    {
        var limit = await _context.ChargeLimits.FirstOrDefaultAsync(limit => limit.CustomerId == customerId);
        if (limit != null)
        {
            _context.ChargeLimits.Remove(limit);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    public async Task<bool> DeleteCurrentChargeLimitAsync(long customerId)
    {
        var limit = await _context.CurrentChargeLimits.FirstOrDefaultAsync(limit => limit.CustomerId == customerId);
        if (limit != null)
        {
            _context.CurrentChargeLimits.Remove(limit);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    public async Task<ServiceResult<SpendLimitResponse>> SpendLimitAsync(SpendLimitRequest spendLimitRequest)
    {
        
        if (decimal.Round(spendLimitRequest.Amount.Value, 2) != spendLimitRequest.Amount)
        {
            return ServiceResult<SpendLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (spendLimitRequest.Amount < 0)
        {
            return ServiceResult<SpendLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        var limits = await (
            from current in _context.CurrentChargeLimits
            join configured in _context.ChargeLimits
                on current.CustomerId equals configured.CustomerId
            where current.CustomerId == spendLimitRequest.CustomerId
            select new
            {
                Current = current,
                Configured = configured
            }
        ).SingleOrDefaultAsync();
        
        if (limits?.Current is null || limits.Configured is null)
        {
            return ServiceResult<SpendLimitResponse>.Failure(
                Errors.LimitNotFoundError
            );
        }

        DateTime now = DateTime.UtcNow;
        
        if (limits.Current.LastDailyReset.Date < now.Date)
        {
            limits.Current.DailyLimit = limits.Configured.DailyLimit;
            limits.Current.LastDailyReset = now;
        }

        if (limits.Current.LastMonthlyReset.Year != now.Year ||
            limits.Current.LastMonthlyReset.Month != now.Month)
        {
            limits.Current.MonthlyLimit = limits.Configured.MonthlyLimit;
            limits.Current.LastMonthlyReset = now;
        }

        if (limits.Current.LastAnnualReset.Year != now.Year)
        {
            limits.Current.AnnualLimit = limits.Configured.AnnualLimit;
            limits.Current.LastAnnualReset = now;
        }

        await _context.SaveChangesAsync();
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE CURRENT_CHARGE_LIMITS
             SET DAILY_LIMIT = DAILY_LIMIT - {spendLimitRequest.Amount}, 
                 MONTHLY_LIMIT = MONTHLY_LIMIT - {spendLimitRequest.Amount},
                 ANNUAL_LIMIT = ANNUAL_LIMIT - {spendLimitRequest.Amount}
             WHERE CUSTOMER_ID = {spendLimitRequest.CustomerId} 
               AND DAILY_LIMIT >= {spendLimitRequest.Amount}
               AND MONTHLY_LIMIT >= {spendLimitRequest.Amount}
               AND ANNUAL_LIMIT >= {spendLimitRequest.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<SpendLimitResponse>.Failure(Errors.InsufficientLimitError);
        }
        
        var limit = await _context.CurrentChargeLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == spendLimitRequest.CustomerId);

        if (limit != null)
        {
            var response = new SpendLimitResponse
            {
                CustomerId = limit.CustomerId,
                NewDailyLimit = limit.DailyLimit,
                NewMonthlyLimit = limit.MonthlyLimit,
                NewAnnualLimit = limit.AnnualLimit,
                TransactionAmount = spendLimitRequest.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<SpendLimitResponse>.Success(response);
        }
        
        else return ServiceResult<SpendLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
}