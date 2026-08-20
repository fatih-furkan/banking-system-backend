using Bank.AuthorizationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AuthorizationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointLogController : ControllerBase
{
    private readonly IPointLogService _pointLogService;

    public PointLogController(IPointLogService pointLogService)
    {
        _pointLogService = pointLogService;
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetLogsByCustomerId(long customerId)
    {
        var logs = await _pointLogService.GetLogsByCustomerIdAsync(customerId);
        return Ok(logs);
    }
}