using Bank.CustomerService.Clients;
using Bank.CustomerService.Data;
using Bank.CustomerService.Models.ClientModels;
using Bank.CustomerService.Models.Dtos;
using Bank.CustomerService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.CustomerService.Services;

public class CustomerService
{
    
    private readonly AppDbContext _context;
    private readonly AccountClient _accountClient;
    private readonly AuthorizationClient _authorizationClient;
    private readonly PointClient _pointClient;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(AppDbContext context, 
        AccountClient accountClient,
        AuthorizationClient authorizationClient,
        PointClient pointClient,
        ILogger<CustomerService> logger)
    {
        _context = context;
        _accountClient = accountClient;
        _authorizationClient = authorizationClient;
        _pointClient = pointClient;
        _logger = logger;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.ToListAsync();
    }
   
    public async Task<Customer?> GetCustomerByCustomerIdAsync(long customerId)
    {
        return await _context.Customers.FindAsync(customerId);
    }

    public async Task<ServiceResult<CreateCustomerResponse>> AddCustomerAsync(
    CreateCustomerRequest request)
    {
        bool onlyDigits = request.Tc.All(char.IsDigit);

        if (!onlyDigits)
        {
            return ServiceResult<CreateCustomerResponse>.Failure(
                Errors.TcError
            );
        }

        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(customer =>
                customer.Tc == request.Tc
            );

        if (existingCustomer is not null)
        {
            return ServiceResult<CreateCustomerResponse>.Failure(
                Errors.TcAssignedError
            );
        }

        long customerId =
            await GetNextCustomerIdSequenceValueAsync();

        Guid operationId = Guid.NewGuid();

        string? pointAccountNo = null;
        
        bool customerCreated = false;
        bool pointAccountCreated = false;
        bool chargeLimitCreated = false;
        bool spendingLimitCreated = false;
        
        try
        {
            var customer = new Customer
            {
                CustomerId = customerId,
                Name = request.Name,
                Surname = request.Surname,
                Tc = request.Tc,

               
                Status = "2"
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            customerCreated = true;

          
            var createPointAccountResult =
                await _pointClient.CreatePointAccountAsync(
                    new CreatePointAccountRequest
                    {
                        CustomerId = customerId
                    }
                );

            if (!createPointAccountResult.IsSuccess)
            {
                throw new GeneralException(
                    createPointAccountResult.Error
                        ?? Errors.PointAccountCreateError,
                    createPointAccountResult.StatusCode
                );
            }

            pointAccountNo = createPointAccountResult.Data;

            pointAccountCreated = true;

          
            var createChargeLimitResult =
                await _accountClient.CreateChargeLimitAsync(
                    new CreateChargeLimitRequest
                    {
                        AnnualLimit =
                            Constants.Limits.AnnualChargeLimit,
                        MonthlyLimit =
                            Constants.Limits.MonthlyChargeLimit,
                        DailyLimit =
                            Constants.Limits.DailyChargeLimit,
                        CustomerId = customerId
                    }
                );

            if (!createChargeLimitResult.IsSuccess)
            {
                throw new GeneralException(
                    createChargeLimitResult.Error
                        ?? Errors.ChargeLimitCreateError,
                    createChargeLimitResult.StatusCode
                );
            }

            chargeLimitCreated = true;

          
            var createSpendingLimitResult =
                await _authorizationClient.AddSpendingLimitAsync(
                    new CreateSpendingLimitRequest
                    {
                        AnnualLimit =
                            Constants.Limits.AnnualSpendingLimit,
                        MonthlyLimit =
                            Constants.Limits.MonthlySpendingLimit,
                        DailyLimit =
                            Constants.Limits.DailySpendingLimit,
                        CustomerId = customerId
                    }
                );

            if (!createSpendingLimitResult.IsSuccess)
            {
                throw new GeneralException(
                    createSpendingLimitResult.Error
                        ?? Errors.SpendingLimitCreateError,
                    createSpendingLimitResult.StatusCode
                );
            }

            spendingLimitCreated = true;

            
            customer.Status = "1";

            await _context.SaveChangesAsync();
            
            return ServiceResult<CreateCustomerResponse>.Success(
                new CreateCustomerResponse
                {
                    Name = customer.Name,
                    Surname = customer.Surname,
                    Tc = customer.Tc,
                    CustomerId = customer.CustomerId,
                    Status = customer.Status
                }
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                Constants.ExceptionMessages.CustomerCreationCompensate);

            await CompensateCustomerCreationAsync(
                customerId,
                spendingLimitCreated,
                chargeLimitCreated,
                pointAccountCreated,
                customerCreated,
                operationId,
                pointAccountNo!
            );

            throw;
        }
    }
    
    private async Task CompensateCustomerCreationAsync(
    long customerId,
    bool spendingLimitCreated,
    bool chargeLimitCreated,
    bool pointAccountCreated,
    bool customerCreated,
    Guid operationId,
    string pointAccountNo)
    {
       
        if (spendingLimitCreated)
        {
            try
            {
                var result =
                    await _authorizationClient
                        .CompensateCreateSpendingLimitAsync(
                            customerId
                        );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Spending limit creation compensation failed. " +
                        "CustomerId: {CustomerId}, " +
                        "OperationId: {OperationId}",
                        customerId,
                        operationId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Spending limit creation compensation threw an exception. " +
                    "CustomerId: {CustomerId}, " +
                    "OperationId: {OperationId}",
                    customerId,
                    operationId
                );
            }
        }

        
        if (chargeLimitCreated)
        {
            try
            {
                var result =
                    await _accountClient
                        .CompensateCreateChargeLimitAsync(
                            customerId
                        );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Charge limit creation compensation failed. " +
                        "CustomerId: {CustomerId}, " +
                        "OperationId: {OperationId}",
                        customerId,
                        operationId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Charge limit creation compensation threw an exception. " +
                    "CustomerId: {CustomerId}, " +
                    "OperationId: {OperationId}",
                    customerId,
                    operationId
                );
            }
        }

        
        if (pointAccountCreated)
        {
            try
            {
                var result =
                    await _pointClient
                        .CompensateCreatePointAccountAsync(
                            pointAccountNo
                        );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Point account creation compensation failed. " +
                        "CustomerId: {CustomerId}, " +
                        "OperationId: {OperationId}",
                        customerId,
                        operationId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Point account creation compensation threw an exception. " +
                    "CustomerId: {CustomerId}, " +
                    "OperationId: {OperationId}",
                    customerId,
                    operationId
                );
            }
        }

        
        if (customerCreated)
        {
            try
            {
                await _context.Customers
                    .Where(customer =>
                        customer.CustomerId == customerId)
                    .ExecuteDeleteAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Customer creation compensation failed. " +
                    "CustomerId: {CustomerId}, " +
                    "OperationId: {OperationId}",
                    customerId,
                    operationId
                );
            }
        }
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