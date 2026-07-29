using Bank.CustomerService.Data;
using Bank.CustomerService.Models.Dtos;
using Bank.CustomerService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.ToListAsync();
    }
    
    public async Task<Customer?> GetCustomerByCustomerIdAsync(long customerId)
    {
        return await _context.Customers.FindAsync(customerId);
    }

    public async Task<ServiceResult<CreateCustomerResponse>> AddCustomerAsync(CreateCustomerRequest createCustomerRequest)
    {
        //Does tc consist of numbers?
        bool onlyDigits = createCustomerRequest.Tc.All(char.IsDigit);
        if (!onlyDigits)
        {
            return ServiceResult<CreateCustomerResponse>.Failure(Errors.TcError);
        }
        
        //Is the tc unique?
        var existingTc = await _context.Customers
            .FirstOrDefaultAsync(account => account.Tc == createCustomerRequest.Tc);
        if (existingTc == null)
        {
            var customerId = await GetNextCustomerIdSequenceValueAsync();
            var customer = new Customer
            {
                CustomerId = customerId,
                Name = createCustomerRequest.Name,
                Surname = createCustomerRequest.Surname,
                Tc = createCustomerRequest.Tc,
                Status = createCustomerRequest.Status
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            CreateCustomerResponse response = new CreateCustomerResponse
            {
                Name = customer.Name,
                Surname = customer.Surname,
                Tc = customer.Tc,
                CustomerId = customer.CustomerId,
                Status = customer.Status
            };
            return ServiceResult<CreateCustomerResponse>.Success(response);
        }

        return ServiceResult<CreateCustomerResponse>.Failure(Errors.TcAssignedError);
    } 

    private async Task<long> GetNextCustomerIdSequenceValueAsync()
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CUSTOMER_ID_SEQ.NEXTVAL FROM DUAL";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }
    
    public async Task<bool> DeleteCustomerAsync(long customerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> CheckExistenceByCustomerIdAsync(long customerId)
    {
        return await _context.Customers.AnyAsync(customer => customer.CustomerId == customerId);
    }
    
    public async Task<ServiceResult<Unit>> AssignStatusAsync(AssignStatusRequest request, long customerId)
    {
        bool isOnlyDigits =
            !string.IsNullOrEmpty(request.Status) &&
            request.Status.All(c => c is >= '0' and <= '9');
        
        if (!isOnlyDigits)
        {
            return ServiceResult<Unit>.Failure(Errors.InvalidStatusError);
        }
        
        var customer = await _context.Customers
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId);
        
        if (customer == null)
        {
            return ServiceResult<Unit>.Failure(Errors.AccountNotFoundError);
        }
        
        customer.Status = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }
}