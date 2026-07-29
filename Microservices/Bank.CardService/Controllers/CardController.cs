using Bank.CardService.Models.Dtos;
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
    public async Task<IActionResult> Add(CreateCardRequest createCardRequest)
    {
        var result = await _cardService.AddCardAsync(createCardRequest);
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

    //olusturulan hesap bagli oldugundan dolayi kart silinemiyor. ne yapmak gerek?
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