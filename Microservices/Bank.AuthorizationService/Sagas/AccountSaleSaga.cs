using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Sagas;

public class AccountSaleSaga
{
    private readonly ILogger<AccountSaleSaga> _logger;
    private readonly AccountClient _accountClient;
    
    public AccountSaleSaga(
        ILogger<AccountSaleSaga> logger,
        AccountClient accountClient)
    {
        _logger = logger;
        _accountClient = accountClient;
    }

    public async Task<ServiceResult<AccountSaleResponse>> ExecuteAsync(AccountSaleRequest request)
    {
        var accountSaleResult =
            await _accountClient.AccountSaleAsync(request);

        if (!accountSaleResult.IsSuccess ||
            accountSaleResult.Data is null)
        {
            return ServiceResult<AccountSaleResponse>
                .Failure(accountSaleResult.Error ?? Errors.AccountSaleError);
        }

        return ServiceResult<AccountSaleResponse>.Success(accountSaleResult.Data);
    }
    
    public async Task CompensateAsync(
        AccountSaleRequest request)
    {
        try
        {
            var result =
                await _accountClient
                    .AccountSaleCompensateAsync(request);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Spending limit compensation failed. " +
                    "AccountNo: {AccountNo}, Error: {Error}",
                    request.AccountNo,
                    result.Error
                );
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages
                    .AccountSaleCompensationError
            );
        }
    }
}