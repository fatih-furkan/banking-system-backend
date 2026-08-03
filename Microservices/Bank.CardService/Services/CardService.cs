using Bank.CardService.Clients;
using Bank.CardService.Data;
using Bank.CardService.Models.Dtos;
using Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;
using Bank.CardService.Models.Entities;
using Bank.CardService.Sagas;
using Bank.CardService.Services.Internal;
using Microsoft.EntityFrameworkCore;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CardService.Services;

public class CardService
{
    private readonly AppDbContext _context;
    private readonly CreateCardSaga _createCardSaga;
    private readonly CustomerClient _customerClient;
    private readonly CardCreator _cardCreator;
    private readonly ILogger<CardService> _logger;
    
    public CardService(AppDbContext context,
        CreateCardSaga createCardSaga,
        CustomerClient customerClient,
        CardCreator cardCreator,
        ILogger<CardService> logger)
    {
        _context = context;
        _createCardSaga = createCardSaga;
        _customerClient = customerClient;
        _cardCreator = cardCreator;
        _logger = logger;
    }

    public async Task<List<Card>> GetAllCardsAsync()
    {
        return await _context.Cards.ToListAsync();
    }
    
    public async Task<Card?> GetCardByCardTokenAsync(string cardToken)
    {
        return await _context.Cards.FindAsync(cardToken);
    }
    
    public async Task<ServiceResult<bool>> CheckExistenceByCardTokenAsync(string cardToken)
    {
        bool exists = await _context.Cards
            .AnyAsync(card => card.CardToken == cardToken);

        return ServiceResult<bool>.Success(exists);
    }

    public async Task<bool> CardBelongsToCustomer(string cardToken, long customerId)
    {
        return await _context.Cards.AnyAsync(card => card.CardToken == cardToken && card.CustomerId == customerId);
    }
    
    public async Task<bool> DeleteCard(string cardToken)
    {
        var card = await _context.Cards.FirstOrDefaultAsync(card => card.CardToken == cardToken);
        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<ServiceResult<CreateCardResponse>> CreateCardAndAccountAsync(CreateCardSagaRequest request)
    {
        var customerExistsResult = await _customerClient.CustomerExistsAsync(request.CustomerId.Value);
        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateCardResponse>
                .Failure(customerExistsResult.Error ?? Errors.CustomerClientError,
                    customerExistsResult.StatusCode);
        }

        if (customerExistsResult.Data == false)
        {
            return ServiceResult<CreateCardResponse>
                .Failure(Errors.CustomerNotFoundError);
        }
        
        
        var cardSagaResult = await _createCardSaga.ExecuteAsync(request);
        if (cardSagaResult.IsSuccess == false)
        {
            return ServiceResult<CreateCardResponse>
                .Failure(cardSagaResult.Error ?? Errors.CardSagaError,
                    cardSagaResult.StatusCode);
        }
        try
        {
            var result = await _cardCreator.CreateCardAsync(new CreateCardRequest
                {
                    AccountNo = cardSagaResult.Data!,
                    BranchCode = request.BranchCode,
                    CustomerId = request.CustomerId
                }
            );
            if (!result.IsSuccess)
            {
                await _createCardSaga.CompensateAsync(cardSagaResult.Data!);
            }

            return result;
        }
        
        catch (Exception e)
        {
            _logger.LogError(
                e,
                Constants.ExceptionMessages.CardSagaError
            );

            await _createCardSaga.CompensateAsync(cardSagaResult.Data!);

            throw;
        }
    }

    public async Task<ServiceResult<string>> FindAccountNoByCardNo(string cardNo)
    {
        var card = await _context.Cards.SingleOrDefaultAsync(card => card.CardNo == cardNo);
        if (card == null)
        {
            return ServiceResult<string>.Failure(Errors.CardNotExistError);
        }

        return ServiceResult<string>.Success(card.CardAccountNo);
    }
}