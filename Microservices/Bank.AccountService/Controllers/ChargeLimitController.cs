using Bank.AccountService.Models.Dtos.Limit;
using Bank.AccountService.Services;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AccountService.Controllers;

[ApiController]
[Route("api/charge-limit")]
public class ChargeLimitController : Controller
{
    private readonly LimitService _limitService;

    public ChargeLimitController(LimitService limitService)
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

    [HttpPost("use-charge-limit")]
    public async Task<IActionResult> UseChargeLimit(UseChargeLimitRequest useChargeLimitRequest)
    {
        var result = await _limitService.UseChargeLimitAsync(useChargeLimitRequest);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }

    [HttpPost("compensate-use-charge-limit")]
    public async Task<IActionResult> CompensateUseChargeLimit(UseChargeLimitRequest useChargeLimitRequest)
    {
        var result = await _limitService.CompensateUseChargeLimitAsync(useChargeLimitRequest);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
}