using Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.CardService.Controllers;

[ApiController]
[Route("api/{controller}")]
public class CardController: ControllerBase
{
    private Services.CardService _cardService;

    public CardController(Services.CardService cardService)
    {
        _cardService = cardService;
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
            return StatusCode(500, new ErrorResponse(Errors.GetCardError));
        }
    }
    
    [HttpGet("{cardToken}")]
    public async Task<IActionResult> GetByCardToken(string cardToken)
    {
        var card = await _cardService.GetCardByCardTokenAsync(cardToken);
        if (card == null)
        {
            return NotFound(new ErrorResponse(Errors.CardNotExistError));
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
        var result = await _cardService.CreateCardAndAccountAsync(createCardSagaRequest);
        if (result.IsSuccess)
        {
            return StatusCode(201, result.Data);
        }
        else return StatusCode(403, new ErrorResponse(result.Error));
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
        else return StatusCode(400, new ErrorResponse(Errors.CardNotExistError));
    }
}