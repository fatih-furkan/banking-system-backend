using Bank.AccountService.Data;
using Bank.AccountService.Models.Dtos;
using Bank.AccountService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error while getting the accounts" + ex);
        }
    }

    [HttpGet("{accountNo}")]
    public async Task<IActionResult> GetByAccountNo(string accountNo)
    {
        var account = await _accountService.GetAccountByAccountNoAsync(accountNo);
        if (account == null)
        {
            return NotFound("Account could not be found.");
        }
        else return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateAccountRequest createAccountRequest)
    {
        var result = await _accountService.AddAccountAsync(createAccountRequest);
        if (result.Data == null)
        {
            return StatusCode(403, result.ErrorMessage);
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
            return StatusCode(403, result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}