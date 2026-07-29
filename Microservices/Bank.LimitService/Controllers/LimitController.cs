using Bank.LimitService.Models.Dtos;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.LimitService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LimitController: Controller
{
    private readonly Services.LimitService _limitService;
    
    public LimitController(Services.LimitService limitService)
    {
        _limitService = limitService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var limits = await _limitService.GetAllChargeLimitsAsync();
        return Ok(limits);
    }

    [HttpGet("{customerId:long}")]
    public async Task<IActionResult> GetByAccountNo(long customerId)
    {
        var limit = await _limitService.GetChargeLimitByCustomerIdAsync(customerId);
        if (limit == null)
        {
            return NotFound(new ErrorResponse(Errors.LimitNotFoundError));
        }
        else return Ok(limit);
    }
    
    [HttpGet("current")]
    public async Task<IActionResult> GetAllCurrent()
    {
        var limits = await _limitService.GetAllCurrentChargeLimitsAsync();
        return Ok(limits);
    }

    [HttpGet("current/{customerId:long}")]
    public async Task<IActionResult> GetCurrentByAccountNo(long customerId)
    {
        var limit = await _limitService.GetCurrentChargeLimitByCustomerIdAsync(customerId);
        if (limit == null)
        {
            return NotFound(new ErrorResponse(Errors.LimitNotFoundError));
        }
        else return Ok(limit);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddChargeLimit(CreateChargeLimitRequest request)
    {
        var result = await _limitService.AddChargeLimitAsync(request);
        if (result.Data == null)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("current/")]
    public async Task<IActionResult> AddCurrentChargeLimit(CreateCurrentChargeLimitRequest request)
    {
        var result = await _limitService.AddCurrentChargeLimitAsync(request);
        if (result.Data == null)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }

    [HttpDelete("{customerId:long}")]
    public async Task<IActionResult> DeleteChargeLimit(long customerId)
    {
        var result = await _limitService.DeleteChargeLimitAsync(customerId);
        if (result == false)
        {
            return StatusCode(403);
        }
        else return Ok();
    }
    
    [HttpDelete("current/{customerId:long}")]
    public async Task<IActionResult> DeleteCurrentChargeLimit(long customerId)
    {
        var result = await _limitService.DeleteCurrentChargeLimitAsync(customerId);
        if (result == false)
        {
            return StatusCode(403);
        }
        else return Ok();
    }
    
    [HttpPost("spend")]
    public async Task<IActionResult> Spend(SpendLimitRequest spendLimitRequest)
    {
        var result = await _limitService.SpendLimitAsync(spendLimitRequest);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
}