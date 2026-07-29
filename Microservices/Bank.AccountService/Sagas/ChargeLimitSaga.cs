using Bank.AccountService.Services;
using Bank.AccountService.Models.Dtos.Limit;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Sagas;

public class ChargeLimitSaga
{
    private readonly ILogger<ChargeLimitSaga> _logger;
    private readonly LimitService _limitService;
    
    public ChargeLimitSaga(
        ILogger<ChargeLimitSaga> logger,
        LimitService limitService)
    {
        _logger = logger;
        _limitService = limitService;
    }
    
    public async Task<ServiceResult<UseChargeLimitResponse>> ExecuteAsync(
        UseChargeLimitRequest request)
    {
        
        var useChargeLimitResult = await _limitService
            .UseChargeLimitAsync(request);
        if (!useChargeLimitResult.IsSuccess || useChargeLimitResult.Data == null)
        {
            return ServiceResult<UseChargeLimitResponse>
                .Failure(useChargeLimitResult.Error ?? Errors.UnexpectedError);
        }
        else return ServiceResult<UseChargeLimitResponse>.Success(useChargeLimitResult.Data);
    }


    public async Task CompensateAsync(UseChargeLimitRequest request)
    {
        try
        {
            await _limitService.CompensateUseChargeLimitAsync(
                request
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages.ChargeLimitCompensationError
            );
        }
    }
}