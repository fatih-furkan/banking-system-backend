using Bank.AuthorizationService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AuthorizationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationController: ControllerBase
{
    private readonly Services.AuthorizationService _authorizationService;
        
    public AuthorizationController(Services.AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var accounts = await _authorizationService.GetAllAuthorizationsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error while getting the authorizations" + ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateAuthorizationRequest request)
    {
        var result = await _authorizationService.CreateAuthorizationAsync(request);
        if (result.Data == null)
        {
            return StatusCode(403, result.ErrorMessage);
        }

        return Ok(result.Data);
    } 
}