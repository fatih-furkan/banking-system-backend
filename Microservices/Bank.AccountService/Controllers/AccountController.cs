using Bank.AccountService.Models.Dtos;
using Bank.AccountService.Models.Dtos.Account;
using Bank.Shared;
using Bank.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Bank.AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController: ControllerBase
{
    private readonly Services.AccountService _accountService;
    
    public AccountController(Services.AccountService accountService)
    {
        _accountService = accountService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        return Ok(accounts);
    }

    [HttpGet("{accountNo}")]
    public async Task<IActionResult> GetByAccountNo(string accountNo)
    {
        var account = await _accountService.GetAccountByAccountNoAsync(accountNo);
        if (account == null)
        {
            return NotFound(new ErrorResponse(Errors.AccountNotFoundError));
        }
        else return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateAccountRequest createAccountRequest)
    {
        var result = await _accountService.AddAccountAsync(createAccountRequest);
        if (result.Data == null)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }

    [HttpDelete("{accountNo}")]
    public async Task<IActionResult> Delete(string accountNo)
    {
        var result = await _accountService.DeleteAccountAsync(accountNo);
        if (result == false)
        {
            return StatusCode(403);
        }
        else return Ok();
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositRequest depositRequest)
    {
        
        var result = await _accountService.DepositAsync(depositRequest);
        if (result.Data == null)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("cash-withdraw")]
    public async Task<IActionResult> CashWithdraw(WithdrawRequest withdrawRequest)
    {
        var result = await _accountService.CashWithdrawAsync(withdrawRequest);
        if (result.Data == null)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("fast-withdraw")]
    public async Task<IActionResult> FastWithdraw(WithdrawRequest withdrawRequest)
    {
        var result = await _accountService.FastWithdrawAsync(withdrawRequest);
        if (result.Data == null)
        {
            return StatusCode(result.StatusCode, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("{accountNo}/assign-status")]
    public async Task<IActionResult> AssignStatus(AssignStatusRequest request, string accountNo)
    {
        var result = await _accountService.AssignStatusAsync(request, accountNo);
        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpGet("{accountNo}/exists")]
    public async Task<ActionResult<bool>> Exists(string accountNo)
    {
        bool exists = await _accountService.CheckExistenceByAccountNoAsync(accountNo);
        return Ok(exists);
    }

    [HttpPost("sale")]
    public async Task<IActionResult> Sale(SaleRequest request)
    {
        var result = await _accountService.SaleAsync(request);
        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
    
    [HttpPost("compensate-sale")]
    public async Task<IActionResult> CompensateSale(SaleRequest request)
    {
        var result = await _accountService.CompensateSaleAsync(request);
        if (!result.IsSuccess)
        {
            return StatusCode(403, new ErrorResponse(result.Error));
        }

        return Ok(result.Data);
    }
}