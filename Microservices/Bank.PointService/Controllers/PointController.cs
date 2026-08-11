using Bank.PointService.Models.Dtos;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.PointService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointController: ControllerBase
{
    private readonly Services.PointService _pointService;

    public PointController(Services.PointService pointService)
    {
        _pointService = pointService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _pointService.GetAllPointAccountsAsync();
        return Ok(accounts);
    }
    
    [HttpGet("{accountNo}")]
    public async Task<IActionResult> GetByAccountNo(string accountNo)
    {
        var account = await _pointService.GetPointAccountByAccountNoAsync(accountNo);
        if (account == null)
        {
            return NotFound(new ErrorResponse(Errors.PointAccountNotFoundError));
        }
        else return Ok(account);
    }
    
    [HttpPost]
    public async Task<IActionResult> Add(CreatePointAccountRequest createPointAccountRequest)
    {
        var result = await _pointService.AddPointAccountAsync(createPointAccountRequest);
        if (result.Data == null)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpDelete("{accountNo}")]
    public async Task<IActionResult> Delete(string accountNo)
    {
        var result = await _pointService.DeletePointAccountAsync(accountNo);
        if (result == false)
        {
            return StatusCode(403);
        }
        else return Ok();
    }
    
    [HttpPost("{accountNo}/assign-status")]
    public async Task<IActionResult> AssignStatus(AssignStatusRequest request, string accountNo)
    {
        var result = await _pointService.AssignStatusAsync(request, accountNo);
        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
}