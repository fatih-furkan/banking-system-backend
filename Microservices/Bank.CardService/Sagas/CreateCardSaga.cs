using Bank.CardService.Clients;
using Bank.CardService.Models.Dtos;
using Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;
using Bank.Shared;

namespace Bank.CardService.Sagas;

public sealed class CreateCardSaga
{
    private readonly AccountClient _accountClient;
    private readonly Services.CardService _cardService;
    private readonly ILogger<CreateCardSaga> _logger;

    public CreateCardSaga(
        AccountClient accountClient,
        Services.CardService cardService,
        ILogger<CreateCardSaga> logger)
    {
        _accountClient = accountClient;
        _cardService = cardService;
        _logger = logger;
    }

    public async Task<ServiceResult<CreateCardResponse>> ExecuteAsync(
        CreateCardSagaRequest request)
    {
        string? accountNo = null;
        
            var createAccountResult = await _accountClient
                .CreateAccountAsync(request.CustomerId, request.BranchCode);
            if (!createAccountResult.IsSuccess || createAccountResult.Data == null)
            {
                return ServiceResult<CreateCardResponse>
                    .Failure(Constants.ExceptionMessages.AccountCreateError);
            }
            else accountNo = createAccountResult.Data.AccountNo;

        try
        {
            var result = await _cardService.CreateCardAsync(new CreateCardRequest
                {
                    AccountNo = accountNo,
                    BranchCode = request.BranchCode,
                    CustomerId = request.CustomerId
                }
            );
            if (!result.IsSuccess)
            {
                await CompensateAsync(accountNo);
            }

            return result;
        }
        
        catch (Exception e)
        {
            _logger.LogError(
                e,
                Constants.ExceptionMessages.CardSagaError
            );

            await CompensateAsync(accountNo);

            throw;
        }
    }


    private async Task CompensateAsync(string accountNo)
    {
        try
        {
            await _accountClient.AssignStatusAsync(
                accountNo, "2"
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages.AccountCompensationError(accountNo),
                accountNo
            );
        }
    }
}