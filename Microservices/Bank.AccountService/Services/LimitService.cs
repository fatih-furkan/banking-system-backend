using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Models.Dtos.Limit;
using Bank.AccountService.Models.Entities.Limit;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Services;

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
    
    public async Task<ServiceResult<UseChargeLimitResponse>> UseChargeLimitAsync(UseChargeLimitRequest useChargeLimitRequest)
    {
        
        if (decimal.Round(useChargeLimitRequest.Amount.Value, 2) != useChargeLimitRequest.Amount)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (useChargeLimitRequest.Amount < 0)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        var limits = await (
            from current in _context.CurrentChargeLimits
            join configured in _context.ChargeLimits
                on current.CustomerId equals configured.CustomerId
            where current.CustomerId == useChargeLimitRequest.CustomerId
            select new
            {
                Current = current,
                Configured = configured
            }
        ).SingleOrDefaultAsync();
        
        if (limits?.Current is null || limits.Configured is null)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(
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
             SET DAILY_LIMIT = DAILY_LIMIT - {useChargeLimitRequest.Amount}, 
                 MONTHLY_LIMIT = MONTHLY_LIMIT - {useChargeLimitRequest.Amount},
                 ANNUAL_LIMIT = ANNUAL_LIMIT - {useChargeLimitRequest.Amount}
             WHERE CUSTOMER_ID = {useChargeLimitRequest.CustomerId} 
               AND DAILY_LIMIT >= {useChargeLimitRequest.Amount}
               AND MONTHLY_LIMIT >= {useChargeLimitRequest.Amount}
               AND ANNUAL_LIMIT >= {useChargeLimitRequest.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(Errors.InsufficientLimitError);
        }
        
        var limit = await _context.CurrentChargeLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == useChargeLimitRequest.CustomerId);

        if (limit != null)
        {
            var response = new UseChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                NewDailyLimit = limit.DailyLimit,
                NewMonthlyLimit = limit.MonthlyLimit,
                NewAnnualLimit = limit.AnnualLimit,
                TransactionAmount = useChargeLimitRequest.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<UseChargeLimitResponse>.Success(response);
        }
        
        else return ServiceResult<UseChargeLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
    
    public async Task<ServiceResult<CompensateUseChargeLimitResponse>> CompensateUseChargeLimitAsync(UseChargeLimitRequest useChargeLimitRequest)
    {
        
        if (decimal.Round(useChargeLimitRequest.Amount.Value, 2) != useChargeLimitRequest.Amount)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (useChargeLimitRequest.Amount < 0)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }

        await _context.SaveChangesAsync();
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE CURRENT_CHARGE_LIMITS
             SET DAILY_LIMIT = DAILY_LIMIT + {useChargeLimitRequest.Amount}, 
                 MONTHLY_LIMIT = MONTHLY_LIMIT + {useChargeLimitRequest.Amount},
                 ANNUAL_LIMIT = ANNUAL_LIMIT + {useChargeLimitRequest.Amount}
             WHERE CUSTOMER_ID = {useChargeLimitRequest.CustomerId} 
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(Errors.CustomerNotExistError);
        }
        
        var limit = await _context.CurrentChargeLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == useChargeLimitRequest.CustomerId);

        if (limit != null)
        {
            var response = new CompensateUseChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                TransactionAmount = useChargeLimitRequest.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<CompensateUseChargeLimitResponse>.Success(response);
        }
        
        else return ServiceResult<CompensateUseChargeLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
}