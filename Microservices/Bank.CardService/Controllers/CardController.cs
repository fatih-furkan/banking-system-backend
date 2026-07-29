using Bank.CardService.Models.Dtos;
using Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;
using Bank.CardService.Sagas;
using Microsoft.AspNetCore.Mvc;

namespace Bank.CardService.Controllers;

[ApiController]
[Route("api/{controller}")]
public class CardController: ControllerBase
{
    private Services.CardService _cardService;
    private readonly CreateCardSaga _createCardSaga;

    public CardController(Services.CardService cardService, CreateCardSaga createCardSaga)
    {
        _cardService = cardService;
        _createCardSaga = createCardSaga;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var cards = await _cardService.GetAllCardsAsync();
            return Ok(cards);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error while getting the cards" + ex);
        }
    }
    
    [HttpGet("{cardToken}")]
    public async Task<IActionResult> GetByCardToken(string cardToken)
    {
        var card = await _cardService.GetCardByCardTokenAsync(cardToken);
        if (card == null)
        {
            return NotFound("Card could not be found.");
        }
        else return Ok(card);
    }
    
    [HttpGet("{cardToken}/exists")]
    public async Task<ActionResult<bool>> Exists(string cardToken)
    {
        bool exists = await _cardService.CheckExistenceByCardTokenAsync(cardToken);
        return Ok(exists);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateCardSagaRequest createCardSagaRequest)
    {
        var result = await _createCardSaga.ExecuteAsync(createCardSagaRequest);
        if (result.IsSuccess)
        {
            return StatusCode(201, result.Data);
        }
        else return StatusCode(403, result.ErrorMessage);
    }
    
    [HttpGet("belongs")]
    public async Task<ActionResult<bool>> CardBelongsToCustomer(
        [FromQuery] string cardToken, 
        [FromQuery] long customerId)
    {
        bool belongs = await _cardService.CardBelongsToCustomer(cardToken, customerId);
        return Ok(belongs);
    }
    
    [HttpDelete("{cardToken}")]
    public async Task<IActionResult> Delete(string cardToken)
    {
        bool result = await _cardService.DeleteCard(cardToken);
        if (result)
        {
            return Ok();
        }
        else return StatusCode(400, "Card does not exist.");
    }
}