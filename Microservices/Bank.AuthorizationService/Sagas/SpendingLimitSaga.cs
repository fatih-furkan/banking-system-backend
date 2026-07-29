using Bank.AuthorizationService.Models.Dtos;
using Bank.AuthorizationService.Services;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Sagas;

public class SpendingLimitSaga
{
    private readonly ILogger<SpendingLimitSaga> _logger;
    private readonly SpendingLimitService _spendingLimitService;

    public SpendingLimitSaga(
        ILogger<SpendingLimitSaga> logger,
        SpendingLimitService spendingLimitService)
    {
        _logger = logger;
        _spendingLimitService = spendingLimitService;
    }

    public async Task<ServiceResult<UseSpendingLimitResponse>> ExecuteAsync(
        UseSpendingLimitRequest request)
    {
        var useSpendingLimitResult =
            await _spendingLimitService.UseSpendingLimitAsync(request);

        if (!useSpendingLimitResult.IsSuccess ||
            useSpendingLimitResult.Data is null)
        {
            return ServiceResult<UseSpendingLimitResponse>.Failure(
                useSpendingLimitResult.Error ?? Errors.UnexpectedError,
                useSpendingLimitResult.StatusCode
            );
        }

        return ServiceResult<UseSpendingLimitResponse>.Success(
            useSpendingLimitResult.Data
        );
    }

    public async Task CompensateAsync(
        UseSpendingLimitRequest request)
    {
        try
        {
            var result =
                await _spendingLimitService
                    .CompensateUseSpendingLimitAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Spending limit compensation failed. " +
                    "CustomerId: {CustomerId}, Error: {Error}",
                    request.CustomerId,
                    result.Error
                );
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages
                    .SpendingLimitCompensationError
            );
        }
    }
}