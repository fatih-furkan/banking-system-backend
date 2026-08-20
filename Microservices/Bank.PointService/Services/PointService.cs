using Bank.PointService.Clients;
using Bank.PointService.Data;
using Bank.PointService.Models.ClientModels;
using Bank.PointService.Models.Dtos;
using Bank.PointService.Models.Entities;
using Bank.Shared;
using Bank.Shared.Constants;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.PointService.Services;

public class PointService
{
    private readonly AppDbContext _context;
    private readonly CustomerClient _customerClient;
    private readonly CardClient _cardClient;
    private readonly AuthorizationClient _authorizationClient;
    private readonly ILogger<PointService> _logger;
    
    public PointService(AppDbContext context,
        CustomerClient customerClient,
        CardClient cardClient,
        AuthorizationClient authorizationClient,
        ILogger<PointService> logger)
    {
        _context = context;
        _customerClient = customerClient;
        _cardClient = cardClient;
        _authorizationClient = authorizationClient;
        _logger = logger;
    }
    
    public async Task<List<PointAccount>> GetAllPointAccountsAsync()
    {
        return await _context.PointAccounts.ToListAsync();
    }
    
    public async Task<PointAccount?> GetPointAccountByAccountNoAsync(string accountNo)
    {
        return await _context.PointAccounts.FindAsync(accountNo);
    }
    
    public async Task<ServiceResult<CreatePointAccountResponse?>> AddPointAccountAsync(
        CreatePointAccountRequest request)
    {
        var customerExistsResult = await _customerClient
            .CustomerExistsAsync(request.CustomerId!.Value);

        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreatePointAccountResponse?>
                .Failure(Errors.CustomerClientError);
        }
        
        if(customerExistsResult.Data == false)
        {
            return ServiceResult<CreatePointAccountResponse?>
                .Failure(Errors.CustomerNotExistError);
        }
        
        var accountNo = await GetNextPointAccountNoSequenceValueAsync();
        var pointAccount = new PointAccount
        {
            AccountNo = accountNo.ToString(),
            CustomerId = request.CustomerId.Value,
            Status = "1",
            EarnedPoint = 0,
            UsedPoint = 0,
            ExpiredPoint = 0
        };
        
        _context.PointAccounts.Add(pointAccount);
        await _context.SaveChangesAsync();
        CreatePointAccountResponse response = new CreatePointAccountResponse
        {
            AccountNo = pointAccount.AccountNo,
            CustomerId = pointAccount.CustomerId,
            Status = pointAccount.Status,
            UsedPoint = pointAccount.UsedPoint,
            EarnedPoint = pointAccount.EarnedPoint,
            ExpiredPoint = pointAccount.ExpiredPoint
        };
        return ServiceResult<CreatePointAccountResponse?>.Success(response);
    }
    
    public async Task<bool> DeletePointAccountAsync(string accountNo)
    {
        var account = await _context.PointAccounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        if (account != null)
        {
            _context.PointAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
    
    public async Task<ServiceResult<Unit>> AssignStatusAsync(AssignStatusRequest request, string accountNo)
    {
        bool isOnlyDigits =
            !string.IsNullOrEmpty(request.Status) &&
            request.Status.All(c => c is >= '0' and <= '9');
        
        if (!isOnlyDigits)
        {
            return ServiceResult<Unit>.Failure(Errors.InvalidStatusError);
        }
        
        var account = await _context.PointAccounts
            .FirstOrDefaultAsync(account => account.AccountNo == accountNo);
        
        if (account == null)
        {
            return ServiceResult<Unit>.Failure(Errors.PointAccountNotFoundError);
        }
        
        account.Status = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }

    public async Task<ServiceResult<Unit>> CompensateCreatePointAccountAsync(string pointAccountNo)
    {
        await _context.PointAccounts
            .Where(x => x.AccountNo == pointAccountNo)
            .ExecuteDeleteAsync();
        
        return ServiceResult<Unit>.Success(new Unit());
    }
    
    private async Task<long> GetNextPointAccountNoSequenceValueAsync()
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT POINT_ACC_NO_SEQ.NEXTVAL FROM DUAL";

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }

    public async Task<ServiceResult<AddPointResponse>> AddPointAsync(AddPointRequest request)
    {

        decimal amount = request.Amount!.Value;

        if (decimal.Round(amount, 2) != amount)
        {
            return ServiceResult<AddPointResponse>.Failure(
                Errors.PrecisionError,
                StatusCodes.Status400BadRequest
            );
        }

        if (amount <= 0)
        {
            return ServiceResult<AddPointResponse>.Failure(
                Errors.NegativeAmountError,
                StatusCodes.Status400BadRequest
            );
        }
        
        var cardExistsResult = await _cardClient.CardExistsByCardNoAsync(request.CardNo);
        
        if (!cardExistsResult.IsSuccess ||
            string.IsNullOrWhiteSpace(cardExistsResult.Data?.CardToken))
        {
            return ServiceResult<AddPointResponse>.Failure(
                cardExistsResult.Error ?? Errors.AccountNotFoundError,
                cardExistsResult.StatusCode
            );
        }

        var cardToken = cardExistsResult.Data.CardToken;

        bool pointAdded = false;

        Guid operationId = Guid.NewGuid();
        
        try
        {

            int affectedRows =
                await _context.PointAccounts
                    .Where(account => account.CustomerId == request.CustomerId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(account => account.EarnedPoint,
                            account => account.EarnedPoint + request.Amount)
                    );

            if (affectedRows == 0)
            {
                throw new GeneralException(
                    Errors.PointAccountNotFoundError,
                    StatusCodes.Status404NotFound
                );
            }

            var thePointAccount = await _context.PointAccounts.SingleOrDefaultAsync(
                account => account.CustomerId == request.CustomerId);

            if (thePointAccount == null)
            {
                throw new GeneralException(
                    Errors.PointAccountNotFoundError
                );
            }

            pointAdded = true;
            
            // var authorizationRequest = new CreateAuthorizationRequest
            // {
            //     AccountNo = null,
            //     Balance = thePointAccount.EarnedPoint,
            //     CardToken = cardToken,
            //     ChannelCode = request.ChannelCode!.Value,
            //     CustomerId = thePointAccount.CustomerId,
            //     Otc = Constants.Otcs.EarnPoint,
            //     Ots = Constants.Ots.EarnPoint.Default,
            //     TransactionAmount = amount,
            //     TransactionDescription = "Earn point",
            //     TransactionStatus = "1",
            //     TransactionId = request.TransactionId
            // };
            
            return ServiceResult<AddPointResponse>.Success(
                new AddPointResponse
                {
                    Amount = request.Amount,
                    EarnedPoint = thePointAccount.EarnedPoint,
                    TransactionId = request.TransactionId!.Value,
                    AccountNo = thePointAccount.AccountNo
                }
            );
        }
        catch (Exception)
        {
            await CompensateAddPointAsync(
                customerId: request.CustomerId!.Value,
                amount: amount,
                depositMade: pointAdded,
                operationId: operationId
            );

            throw;
        }
    }
    
    private async Task CompensateAddPointAsync(
    Guid operationId,
    long customerId,
    decimal amount,
    bool depositMade)
    {
        
        try
        {
            if (depositMade)
            {
                await CompensateAddPointEarnedPointAsync(
                    operationId,
                    customerId,
                    amount
                );
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Local add point compensation failed. " +
                "OperationId: {OperationId}, CustomerId: {CustomerId}",
                operationId,
                customerId
            );
        }
    }
    
    private async Task CompensateAddPointEarnedPointAsync(
        Guid operationId,
        long customerId,
        decimal amount)
    {
        const string operationType =
            Constants.CompensationOperationTypes
                .ReverseAddPointEarnedPoint;

        bool alreadyCompleted =
            await _context.CompletedSagaOperations.AnyAsync(
                operation =>
                    operation.OperationId == operationId &&
                    operation.OperationType == operationType
            );

        if (alreadyCompleted)
        {
            return;
        }

        int affectedRows = await _context.PointAccounts
            .Where(account => account.CustomerId == customerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    account => account.EarnedPoint,
                    account => account.EarnedPoint - amount
                )
            );

        if (affectedRows == 0)
        {
            throw new GeneralException(
                Errors.AccountNotFoundError,
                StatusCodes.Status404NotFound
            );
        }

        _context.CompletedSagaOperations.Add(
            new CompletedSagaOperation
            {
                OperationId = operationId,
                OperationType = operationType,
                CompletedAt = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync();
    }
}