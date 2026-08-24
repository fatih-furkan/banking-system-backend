using Bank.CardService.Clients;
using Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CardService.Sagas;

public sealed class CreateCardSaga
{
    private readonly AccountClient _accountClient;
    private readonly ILogger<CreateCardSaga> _logger;

    public CreateCardSaga(
        AccountClient accountClient,
        ILogger<CreateCardSaga> logger)
    {
        _accountClient = accountClient;
        _logger = logger;
    }
    
    public async Task<ServiceResult<string>> ExecuteAsync(
        CreateCardSagaRequest request)
    {
        string? accountNo = null;
        
            var createAccountResult = await _accountClient
                .CreateAccountAsync(request.CustomerId.Value, request.BranchCode);
            if (!createAccountResult.IsSuccess || createAccountResult.Data == null)
            {
                return ServiceResult<string>
                    .Failure(Errors.AccountCreateError);
            }
            else return ServiceResult<string>.Success(createAccountResult.Data.AccountNo);
    }


    public async Task CompensateAsync(string accountNo)
    {
        try
        {
            await _accountClient.AssignStatusAsync(
                accountNo, "0"
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages.AccountCompensationError
            );
        }
    }
}