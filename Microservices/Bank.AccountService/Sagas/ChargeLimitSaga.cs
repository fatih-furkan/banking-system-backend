using Bank.AccountService.Services;
using Bank.AccountService.Models.Dtos.Limit;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Sagas;

public class ChargeLimitSaga
{
    private readonly ILogger<ChargeLimitSaga> _logger;
    private readonly ChargeLimitService _chargeLimitService;
    
    public ChargeLimitSaga(
        ILogger<ChargeLimitSaga> logger,
        ChargeLimitService chargeLimitService)
    {
        _logger = logger;
        _chargeLimitService = chargeLimitService;
    }
    
    public async Task<ServiceResult<UseChargeLimitResponse>> ExecuteAsync(
        UseChargeLimitRequest request)
    {
        
        var useChargeLimitResult = await _chargeLimitService
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
            await _chargeLimitService.CompensateUseChargeLimitAsync(
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