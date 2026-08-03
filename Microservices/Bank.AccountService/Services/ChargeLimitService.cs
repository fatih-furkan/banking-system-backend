using Bank.AccountService.Clients;
using Bank.AccountService.Data;
using Bank.AccountService.Models.Dtos.Limit;
using Bank.AccountService.Models.Entities.Limit;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Services;

public class ChargeLimitService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;

    public ChargeLimitService(AppDbContext appDbContext, CustomerClient customerClient)
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
        var customerExistsResult = await _customerClient
            .CustomerExistsAsync(createChargeLimitRequest.CustomerId!.Value);

        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateChargeLimitResponse?>
                .Failure(Errors.CustomerClientError);
        }
        
        if(customerExistsResult.Data == false)
        {
            return ServiceResult<CreateChargeLimitResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        
        ChargeLimit? chargeLimit = 
            await _context.ChargeLimits.FindAsync(createChargeLimitRequest.CustomerId);
        
        //if limits already exist for this customer
        if ( chargeLimit != null)
        {
            return ServiceResult<CreateChargeLimitResponse?>
                .Failure(Errors.LimitAlreadyExistsError);
        }

        DateTime time = DateTime.UtcNow;
        
        var limit = new ChargeLimit
        {
            CustomerId = createChargeLimitRequest.CustomerId.Value,
            AnnualLimit = createChargeLimitRequest.AnnualLimit!.Value,
            DailyLimit = createChargeLimitRequest.DailyLimit!.Value,
            MonthlyLimit = createChargeLimitRequest.MonthlyLimit!.Value
        };
        
        var currentLimit = new CurrentChargeLimit
        {
            CustomerId = createChargeLimitRequest.CustomerId.Value,
            AnnualLimit = createChargeLimitRequest.AnnualLimit.Value,
            DailyLimit = createChargeLimitRequest.DailyLimit.Value,
            MonthlyLimit = createChargeLimitRequest.MonthlyLimit.Value,
            LastAnnualReset = time,
            LastDailyReset = time,
            LastMonthlyReset = time
        };
        
        _context.ChargeLimits.Add(limit);
        _context.CurrentChargeLimits.Add(currentLimit);
        await _context.SaveChangesAsync();
        
        CreateChargeLimitResponse response = new CreateChargeLimitResponse
        {
            CustomerId = limit.CustomerId,
            AnnualLimit = limit.AnnualLimit,
            MonthlyLimit = limit.MonthlyLimit,
            DailyLimit = limit.DailyLimit,
            CurrentAnnualLimit = currentLimit.AnnualLimit,
            CurrentMonthlyLimit = currentLimit.MonthlyLimit,
            CurrentDailyLimit =  currentLimit.DailyLimit
        };
        return ServiceResult<CreateChargeLimitResponse?>.Success(response);
    }
    
    /*
     * Add charge limit function already creates an entry for both of the charge limit tables.
     * Thus, this function should only be used in special cases.
     */
    public async Task<ServiceResult<CreateCurrentChargeLimitResponse?>> AddCurrentChargeLimitAsync(
        CreateCurrentChargeLimitRequest createCurrentChargeLimitRequest)
    {
        var customerExistsResult = await _customerClient
            .CustomerExistsAsync(createCurrentChargeLimitRequest.CustomerId!.Value);

        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateCurrentChargeLimitResponse?>
                .Failure(customerExistsResult.Error ?? Errors.CustomerClientError,
                    customerExistsResult.StatusCode);
        }
        
        if(customerExistsResult.Data == false)
        {
            return ServiceResult<CreateCurrentChargeLimitResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        
        var limit = new CurrentChargeLimit
        {
            CustomerId = createCurrentChargeLimitRequest.CustomerId.Value,
            AnnualLimit = 0,
            DailyLimit = 0,
            MonthlyLimit = 0
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
    
    public async Task<ServiceResult<UseChargeLimitResponse>> UseChargeLimitAsync(UseChargeLimitRequest request)
    {
        
        if (decimal.Round(request.Amount.Value, 2) != request.Amount)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (request.Amount < 0)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }
        
        var limits = await (
            from current in _context.CurrentChargeLimits
            join configured in _context.ChargeLimits
                on current.CustomerId equals configured.CustomerId
            where current.CustomerId == request.CustomerId
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
        
        int affectedRows = await _context.CurrentChargeLimits
            .Where(limit =>
                limit.CustomerId == request.CustomerId &&
                limit.DailyLimit >= request.Amount &&
                limit.MonthlyLimit >= request.Amount &&
                limit.AnnualLimit >= request.Amount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    limit => limit.DailyLimit,
                    limit => limit.DailyLimit - request.Amount)
                .SetProperty(
                    limit => limit.MonthlyLimit,
                    limit => limit.MonthlyLimit - request.Amount)
                .SetProperty(
                    limit => limit.AnnualLimit,
                    limit => limit.AnnualLimit - request.Amount)
            );

        if (affectedRows == 0)
        {
            return ServiceResult<UseChargeLimitResponse>.Failure(Errors.InsufficientLimitError);
        }
        
        var limit = await _context.CurrentChargeLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == request.CustomerId);

        if (limit != null)
        {
            var response = new UseChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                NewDailyLimit = limit.DailyLimit,
                NewMonthlyLimit = limit.MonthlyLimit,
                NewAnnualLimit = limit.AnnualLimit,
                TransactionAmount = request.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<UseChargeLimitResponse>.Success(response);
        }
        
        else return ServiceResult<UseChargeLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
    
    public async Task<ServiceResult<CompensateUseChargeLimitResponse>> CompensateUseChargeLimitAsync(UseChargeLimitRequest request)
    {
        
        if (decimal.Round(request.Amount.Value, 2) != request.Amount)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(
                Errors.PrecisionError, 403);
        }

        if (request.Amount < 0)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(
                Errors.NegativeAmountError, 403);
        }

        await _context.SaveChangesAsync();
        
        int affectedRows = await _context.CurrentChargeLimits
            .Where(limit =>
                limit.CustomerId == request.CustomerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    limit => limit.DailyLimit,
                    limit => limit.DailyLimit + request.Amount)
                .SetProperty(
                    limit => limit.MonthlyLimit,
                    limit => limit.MonthlyLimit + request.Amount)
                .SetProperty(
                    limit => limit.AnnualLimit,
                    limit => limit.AnnualLimit + request.Amount)
            );

        if (affectedRows == 0)
        {
            return ServiceResult<CompensateUseChargeLimitResponse>.Failure(Errors.CustomerNotExistError);
        }
        
        var limit = await _context.CurrentChargeLimits
            .AsNoTracking()
            .FirstOrDefaultAsync
            (limit => limit.CustomerId == request.CustomerId);

        if (limit != null)
        {
            var response = new CompensateUseChargeLimitResponse
            {
                CustomerId = limit.CustomerId,
                TransactionAmount = request.Amount,
                TransactionTime = DateTime.UtcNow
            };
            
            return ServiceResult<CompensateUseChargeLimitResponse>.Success(response);
        }
        
        else return ServiceResult<CompensateUseChargeLimitResponse>.Failure(Errors.AccountNotFoundError);
    }
}