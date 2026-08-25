using Bank.AuthorizationService.Models;
using Bank.AuthorizationService.Models.Dtos;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AuthorizationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationController : ControllerBase
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
            return StatusCode(500, new ErrorResponse(Errors.AuthorizationGetError));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateAuthorizationRequest request)
    {
        var result = await _authorizationService.CreateAuthorizationAsync(request);
        if (result.Data == null)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return StatusCode(201, result.Data);
    }

    [HttpPatch("{guid}/assign-status")]
    public async Task<IActionResult> AssignStatus(AssignStatusRequest request, string guid)
    {
        var result = await _authorizationService.AssignStatusAsync(request, guid);
        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }

    [HttpPost("sale")]
    public async Task<IActionResult> Sale(SaleRequest request)
    {
        var result = await _authorizationService.SaleAsync(request);

        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("refund")]
    public async Task<IActionResult> Refund(RefundRequest request)
    {
        var result = await _authorizationService.RefundAsync(request);

        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
}