using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Data;
using Bank.AuthorizationService.Models.Dtos;
using Bank.AuthorizationService.Models.Entities;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.AuthorizationService.Services;

public class SpendingLimitService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;

    public SpendingLimitService(AppDbContext appDbContext, CustomerClient customerClient)
    {
        _context = appDbContext;
        _customerClient = customerClient;
    }
    
    public async Task<List<SpendingLimit>> GetAllSpendingLimitsAsync()
    {
        return await _context.SpendingLimits.ToListAsync();
    }
    
    public async Task<SpendingLimit?> GetSpendingLimitByCustomerIdAsync(long customerId)
    {
        return await _context.SpendingLimits.FindAsync(customerId);
    }
    
    public async Task<List<CurrentSpendingLimit>> GetAllCurrentSpendingLimitsAsync()
    {
        return await _context.CurrentSpendingLimits.ToListAsync();
    }
    
    public async Task<CurrentSpendingLimit?> GetCurrentSpendingLimitByCustomerIdAsync(long customerId)
    {
        return await _context.CurrentSpendingLimits.FindAsync(customerId);
    }
    
    public async Task<ServiceResult<CreateSpendingLimitResponse?>> AddSpendingLimitAsync(
        CreateSpendingLimitRequest createSpendingLimitRequest)
    {
        bool customerExists = await _customerClient
            .CustomerExistsAsync(createSpendingLimitRequest.CustomerId.Value);
        
        if(!customerExists)
        {
            return ServiceResult<CreateSpendingLimitResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        else
        {
            var limit = new SpendingLimit
            {
                CustomerId = createSpendingLimitRequest.CustomerId.Value,
                AnnualLimit = createSpendingLimitRequest.AnnualLimit!.Value,
                DailyLimit = createSpendingLimitRequest.DailyLimit!.Value,
                MonthlyLimit = createSpendingLimitRequest.MonthlyLimit!.Value
            };
            
            var currentLimit = new CurrentSpendingLimit
            {
                CustomerId = createSpendingLimitRequest.CustomerId.Value,
                AnnualLimit = createSpendingLimitRequest.AnnualLimit.Value,
                DailyLimit = createSpendingLimitRequest.DailyLimit.Value,
                MonthlyLimit = createSpendingLimitRequest.MonthlyLimit.Value
            };
            
            _context.SpendingLimits.Add(limit);
            _context.CurrentSpendingLimits.Add(currentLimit);
            await _context.SaveChangesAsync();
            
            CreateSpendingLimitResponse response = new CreateSpendingLimitResponse
            {
                CustomerId = limit.CustomerId,
                AnnualLimit = limit.AnnualLimit,
                MonthlyLimit = limit.MonthlyLimit,
                DailyLimit = limit.DailyLimit,
                CurrentAnnualLimit = currentLimit.AnnualLimit,
                CurrentMonthlyLimit = currentLimit.MonthlyLimit,
                CurrentDailyLimit =  currentLimit.DailyLimit
            };
            return ServiceResult<CreateSpendingLimitResponse?>.Success(response);
        }
    }
    
    public async Task<bool> DeleteSpendingLimitAsync(long customerId)
    {
        var limit = await _context.SpendingLimits
            .FirstOrDefaultAsync(limit => limit.CustomerId == customerId);
        if (limit != null)
        {
            _context.SpendingLimits.Remove(limit);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    public async Task<bool> DeleteCurrentSpendingLimitAsync(long customerId)
    {
        var limit = await _context.CurrentSpendingLimits
            .FirstOrDefaultAsync(limit => limit.CustomerId == customerId);
        if (limit != null)
        {
            _context.CurrentSpendingLimits.Remove(limit);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    public async Task<ServiceResult<UseSpendingLimitResponse>> UseSpendingLimitAsync(UseSpendingLimitRequest useSpendingLimitRequest)
    {
        
        if (decimal.Round(useSpendingLimitRequest.Amount.Value, 2) != useSpendingLimitRequest.Amount)
        {
            return ServiceResult<UseSpendingLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (useSpendingLimitRequest.Amount < 0)
        {
            return ServiceResult<UseSpendingLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        var limits = await (
            from current in _context.CurrentSpendingLimits
            join configured in _context.SpendingLimits
                on current.CustomerId equals configured.CustomerId
            where current.CustomerId == useSpendingLimitRequest.CustomerId
            select new
            {
                Current = current,
                Configured = configured
            }
        ).SingleOrDefaultAsync();
        
        if (limits?.Current is null || limits.Configured is null)
        {
            return ServiceResult<UseSpendingLimitResponse>.Failure(
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
             UPDATE CURRENT_SPENDING_LIMITS
             SET DAILY_LIMIT = DAILY_LIMIT - {useSpendingLimitRequest.Amount}, 
                 MONTHLY_LIMIT = MONTHLY_LIMIT - {useSpendingLimitRequest.Amount},
                 ANNUAL_LIMIT = ANNUAL_LIMIT - {useSpendingLimitRequest.Amount}
             WHERE CUSTOMER_ID = {useSpendingLimitRequest.CustomerId} 
               AND DAILY_LIMIT >= {useSpendingLimitRequest.Amount}
               AND MONTHLY_LIMIT >= {useSpendingLimitRequest.Amount}
               AND ANNUAL_LIMIT >= {useSpendingLimitRequest.Amount}
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<UseSpendingLimitResponse>.Failure(Errors.InsufficientLimitError);
        }
        
        var limit = await _context.CurrentSpendingLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == useSpendingLimitRequest.CustomerId);

        if (limit != null)
        {
            var response = new UseSpendingLimitResponse
            {
                CustomerId = limit.CustomerId,
                NewDailyLimit = limit.DailyLimit,
                NewMonthlyLimit = limit.MonthlyLimit,
                NewAnnualLimit = limit.AnnualLimit,
                TransactionAmount = useSpendingLimitRequest.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<UseSpendingLimitResponse>.Success(response);
        }
        
        else return ServiceResult<UseSpendingLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
    
    public async Task<ServiceResult<CompensateUseSpendingLimitResponse>> CompensateUseSpendingLimitAsync(UseSpendingLimitRequest useSpendingLimitRequest)
    {
        
        if (decimal.Round(useSpendingLimitRequest.Amount.Value, 2) != useSpendingLimitRequest.Amount)
        {
            return ServiceResult<CompensateUseSpendingLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (useSpendingLimitRequest.Amount < 0)
        {
            return ServiceResult<CompensateUseSpendingLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }

        await _context.SaveChangesAsync();
        
        int affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             UPDATE CURRENT_SPENDING_LIMITS
             SET DAILY_LIMIT = DAILY_LIMIT + {useSpendingLimitRequest.Amount}, 
                 MONTHLY_LIMIT = MONTHLY_LIMIT + {useSpendingLimitRequest.Amount},
                 ANNUAL_LIMIT = ANNUAL_LIMIT + {useSpendingLimitRequest.Amount}
             WHERE CUSTOMER_ID = {useSpendingLimitRequest.CustomerId} 
             """
        );

        if (affectedRows == 0)
        {
            return ServiceResult<CompensateUseSpendingLimitResponse>.Failure(Errors.CustomerNotExistError);
        }
        
        var limit = await _context.CurrentSpendingLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == useSpendingLimitRequest.CustomerId);

        if (limit != null)
        {
            var response = new CompensateUseSpendingLimitResponse
            {
                CustomerId = limit.CustomerId,
                TransactionAmount = useSpendingLimitRequest.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<CompensateUseSpendingLimitResponse>.Success(response);
        }
        
        else return ServiceResult<CompensateUseSpendingLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
}