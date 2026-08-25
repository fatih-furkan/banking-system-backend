using Bank.AuthorizationService.Models.Dtos;
using Bank.AuthorizationService.Services;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AuthorizationService.Controllers;

[ApiController]
[Route("api/spending-limit")]
public class SpendingLimitController : ControllerBase
{
    private readonly SpendingLimitService _spendingLimitService;

    public SpendingLimitController(
        SpendingLimitService spendingLimitService)
    {
        _spendingLimitService = spendingLimitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var spendingLimits =
            await _spendingLimitService.GetAllSpendingLimitsAsync();

        return Ok(spendingLimits);
    }

    [HttpGet("{customerId:long}")]
    public async Task<IActionResult> GetByCustomerId(long customerId)
    {
        var spendingLimit =
            await _spendingLimitService
                .GetSpendingLimitByCustomerIdAsync(customerId);

        if (spendingLimit is null)
        {
            return NotFound(
                new ErrorResponse(Errors.LimitNotFoundError)
            );
        }

        return Ok(spendingLimit);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetAllCurrent()
    {
        var currentSpendingLimits =
            await _spendingLimitService
                .GetAllCurrentSpendingLimitsAsync();

        return Ok(currentSpendingLimits);
    }

    [HttpGet("current/{customerId:long}")]
    public async Task<IActionResult> GetCurrentByCustomerId(
        long customerId)
    {
        var currentSpendingLimit =
            await _spendingLimitService
                .GetCurrentSpendingLimitByCustomerIdAsync(customerId);

        if (currentSpendingLimit is null)
        {
            return NotFound(
                new ErrorResponse(Errors.LimitNotFoundError)
            );
        }

        return Ok(currentSpendingLimit);
    }

    [HttpPost]
    public async Task<IActionResult> AddSpendingLimit(
        CreateSpendingLimitRequest request)
    {
        var result =
            await _spendingLimitService.AddSpendingLimitAsync(request);

        if (!result.IsSuccess)
        {
            return StatusCode(
                result.StatusCode,
                new ErrorResponse(result.Error)
            );
        }

        return StatusCode(201, result.Data);
    }

    [HttpDelete("{customerId:long}")]
    public async Task<IActionResult> DeleteSpendingLimit(
        long customerId)
    {
        bool deleted =
            await _spendingLimitService
                .DeleteSpendingLimitAsync(customerId);

        if (!deleted)
        {
            return NotFound(
                new ErrorResponse(Errors.LimitNotFoundError)
            );
        }

        return NoContent();
    }

    [HttpDelete("current/{customerId:long}")]
    public async Task<IActionResult> DeleteCurrentSpendingLimit(
        long customerId)
    {
        bool deleted =
            await _spendingLimitService
                .DeleteCurrentSpendingLimitAsync(customerId);

        if (!deleted)
        {
            return NotFound(
                new ErrorResponse(Errors.LimitNotFoundError)
            );
        }

        return NoContent();
    }

    [HttpPost("use-spending-limit")]
    public async Task<IActionResult> UseSpendingLimit(
        UseSpendingLimitRequest request)
    {
        var result =
            await _spendingLimitService.UseSpendingLimitAsync(request);

        if (!result.IsSuccess)
        {
            return StatusCode(
                result.StatusCode,
                new ErrorResponse(result.Error)
            );
        }

        return Ok(result.Data);
    }

    [HttpPost("compensate-use-spending-limit")]
    public async Task<IActionResult> CompensateUseSpendingLimit(
        CompensateUseSpendingLimitRequest request)
    {
        var result =
            await _spendingLimitService
                .CompensateUseSpendingLimitAsync(request);

        if (!result.IsSuccess)
        {
            return StatusCode(
                result.StatusCode,
                new ErrorResponse(result.Error)
            );
        }

        return Ok(result.Data);
    }
    
    [HttpPost("compensate-create-spending-limit")]
    public async Task<IActionResult> CompensateCreateSpendingLimit(
        [FromBody] long customerId)
    {
        var result =
            await _spendingLimitService
                .CompensateCreateSpendingLimitAsync(customerId);

        if (!result.IsSuccess)
        {
            return StatusCode(
                result.StatusCode,
                new ErrorResponse(result.Error)
            );
        }

        return Ok(result.Data);
    }
}