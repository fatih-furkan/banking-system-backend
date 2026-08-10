using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AuthorizationService.Sagas;

public class AccountRefundSaga
{
    private readonly ILogger<AccountRefundSaga> _logger;
    private readonly AccountClient _accountClient;
    
    public AccountRefundSaga(
        ILogger<AccountRefundSaga> logger,
        AccountClient accountClient)
    {
        _logger = logger;
        _accountClient = accountClient;
    }

    public async Task<ServiceResult<AccountRefundResponse>> ExecuteAsync(AccountRefundRequest request)
    {
        var accountRefundResult =
            await _accountClient.AccountRefundAsync(request);

        if (!accountRefundResult.IsSuccess ||
            accountRefundResult.Data is null)
        {
            return ServiceResult<AccountRefundResponse>
                .Failure(accountRefundResult.Error ?? Errors.AccountRefundError);
        }

        return ServiceResult<AccountRefundResponse>.Success(accountRefundResult.Data);
    }
}