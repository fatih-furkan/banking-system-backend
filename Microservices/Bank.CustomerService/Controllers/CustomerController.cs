using Bank.CustomerService.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Bank.CustomerService.Controllers;

[ApiController]
[Route("api/{controller}")]
public class CustomerController: ControllerBase
{
    private Services.CustomerService _customerService;

    public CustomerController(Services.CustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var accounts = await _customerService.GetAllCustomersAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error while getting the accounts" + ex);
        }
    }
    
    [HttpGet("{customerId:long}")]
    public async Task<IActionResult> GetByCustomerId(long customerId)
    {
        var customer = await _customerService.GetCustomerByCustomerIdAsync(customerId);
        if (customer == null)
        {
            return NotFound("Customer could not be found.");
        }
        else return Ok(customer);
    }
    
    [HttpPost]
    public async Task<IActionResult> Add(CreateCustomerRequest createCustomerRequest)
    {
        var result = await _customerService.AddCustomerAsync(createCustomerRequest);
        if (result.Data == null)
        {
            return StatusCode(403, result.ErrorMessage);
        }

        return Ok(result.Data);
    }
    
    [HttpDelete("{customerId:long}")]
    public async Task<IActionResult> Delete(long customerId)
    {
        var result = await _customerService.DeleteCustomerAsync(customerId);
        if (result == false)
        {
            return NotFound();
        }
        else return Ok();
    }
    
    [HttpGet("{customerId:long}/exists")]
    public async Task<ActionResult<bool>> Exists(long customerId)
    {
        bool exists = await _customerService.CheckExistenceByCustomerIdAsync(customerId);
        return Ok(exists);
    }
    
    [HttpPost("{customerId:long}/assign-status")]
    public async Task<IActionResult> AssignStatus(AssignStatusRequest request, long customerId)
    {
        var result = await _customerService.AssignStatusAsync(request, customerId);
        if (result.Data == null)
        {
            return StatusCode(403, result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}