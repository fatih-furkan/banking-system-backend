using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Data;
using Bank.AuthorizationService.Models;
using Bank.AuthorizationService.Models.Dtos;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.AuthorizationService.Models.Entities;
using Bank.AuthorizationService.Sagas;
using Bank.Shared;
using Bank.Shared.Constants;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.AuthorizationService.Services;

public class AuthorizationService
{
    private readonly AppDbContext _context;
    private readonly CardClient _cardClient;
    private readonly AccountClient _accountClient;
    private readonly CustomerClient _customerClient;
    private readonly SpendingLimitSaga _spendingLimitSaga;
    private readonly AccountSaleSaga _accountSaleSaga;
    private readonly ILogger<AuthorizationService> _logger;
    
    public AuthorizationService(AppDbContext context, 
        CardClient cardClient,
        AccountClient accountClient,
        SpendingLimitSaga spendingLimitSaga,
        AccountSaleSaga accountSaleSaga,
        ILogger<AuthorizationService> logger,
        CustomerClient customerClient)
    {
        _context = context;
        _cardClient = cardClient;
        _accountClient = accountClient;
        _customerClient = customerClient;
        _spendingLimitSaga = spendingLimitSaga;
        _accountSaleSaga = accountSaleSaga;
        _logger = logger;
    }
    
    public async Task<List<Authorization>> GetAllAuthorizationsAsync()
    {
        return await _context.Authorizations.ToListAsync();
    }

    public async Task<ServiceResult<CreateAuthorizationResponse>> CreateAuthorizationAsync(
        CreateAuthorizationRequest request)
    {
        //account existance
        var accountExistsResult = await _accountClient.AccountExistsAsync(request.AccountNo);
        if (!accountExistsResult.IsSuccess)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(Errors.AccountClientError);
        }
        
        if (accountExistsResult.Data == false)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(Errors.AccountNotFoundError);
        }

        //card existance
        if (request.CardToken != null)
        {
            var cardExistsResult = await _cardClient.CardExistsAsync(request.CardToken);
            if (!cardExistsResult.IsSuccess)
            {
                return ServiceResult<CreateAuthorizationResponse>
                    .Failure(Errors.CardClientError);
            }
        
            if (cardExistsResult.Data == false)
            {
                return ServiceResult<CreateAuthorizationResponse>
                    .Failure(Errors.CardNotFoundError);
            }
        }
        
        //customer existance
        var customerExistsResult = await _customerClient.CustomerExistsAsync(request.CustomerId!.Value);
        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(Errors.CustomerClientError);
        }
        
        if (customerExistsResult.Data == false)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(Errors.CustomerNotFoundError);
        }
        
        Authorization auth = new Authorization
        {
            Balance = request.Balance,
            AccountNo = request.AccountNo,
            CardToken = request.CardToken,
            ChannelCode = request.ChannelCode,
            CustomerId = request.CustomerId.Value,
            Guid = Guid.NewGuid().ToString(),
            Otc = request.Otc,
            Ots = request.Ots,
            TransactionAmount = request.TransactionAmount,
            TransactionDate = DateTime.UtcNow,
            TransactionStatus = request.TransactionStatus,
            TransactionDescription = request.TransactionDescription,
            TransactionId = request.TransactionId!.Value
        };
        
        bool transactionExists = await _context.Authorizations
            .AnyAsync(a => a.TransactionId == request.TransactionId);

        if (transactionExists)
        {
            return ServiceResult<CreateAuthorizationResponse>.Failure(
                Errors.TransactionAlreadyExistsError,
                StatusCodes.Status409Conflict
            );
        }
        
        _context.Add(auth);
        await _context.SaveChangesAsync();
        
        CreateAuthorizationResponse response = new CreateAuthorizationResponse
        {
            Balance = auth.Balance,
            AccountNo = auth.AccountNo,
            CardToken = auth.CardToken,
            ChannelCode = auth.ChannelCode,
            CustomerId = auth.CustomerId,
            Guid = auth.Guid,
            Otc = auth.Otc,
            Ots = auth.Ots,
            TransactionAmount = auth.TransactionAmount,
            TransactionDate = auth.TransactionDate,
            TransactionStatus = auth.TransactionStatus,
            TransactionDescription = auth.TransactionDescription,
            TransactionId = auth.TransactionId
        };
        return ServiceResult<CreateAuthorizationResponse>.Success(response);
    }
    
    public async Task<ServiceResult<Unit>> AssignStatusAsync(AssignStatusRequest request, string guid)
    {
        bool isOnlyDigits =
            !string.IsNullOrEmpty(request.Status) &&
            request.Status.All(c => c is >= '0' and <= '9');
        
        if (!isOnlyDigits)
        {
            return ServiceResult<Unit>.Failure(Errors.InvalidStatusError);
        }
        
        var auth = await _context.Authorizations
            .FirstOrDefaultAsync(auth => auth.Guid == guid);
        
        if (auth == null)
        {
            return ServiceResult<Unit>.Failure(Errors.AuthorizationGetError);
        }
        
        auth.TransactionStatus = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }

    public async Task<ServiceResult<SaleResponse>> SaleAsync(
    SaleRequest request)
    {
        decimal amount = request.Amount.Value;

        if (decimal.Round(amount, 2) != amount)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.PrecisionError,
                StatusCodes.Status400BadRequest
            );
        }

        if (amount <= 0)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.NegativeAmountError,
                StatusCodes.Status400BadRequest
            );
        }

        if (request.ChannelCode != ChannelCode.Fast &&
            request.ChannelCode != ChannelCode.Online &&
            request.ChannelCode != ChannelCode.Pos)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.UnauthorizedChannelError,
                StatusCodes.Status403Forbidden
            );
        }

        var accountNoResult =
            await _cardClient.FindAccountNoByCardNoAsync(request.CardNo);

        if (!accountNoResult.IsSuccess ||
            string.IsNullOrWhiteSpace(accountNoResult.Data))
        {
            return ServiceResult<SaleResponse>.Failure(
                accountNoResult.Error ?? Errors.AccountNotFoundError,
                accountNoResult.StatusCode
            );
        }

        string accountNo = accountNoResult.Data;

        var customerIdResult =
            await _accountClient.GetCustomerIdAsync(accountNo);

        if (!customerIdResult.IsSuccess ||
            customerIdResult.Data is null)
        {
            return ServiceResult<SaleResponse>.Failure(
                customerIdResult.Error ?? Errors.CustomerNotFoundError,
                customerIdResult.StatusCode
            );
        }

        long customerId = customerIdResult.Data.Value;

        var limitRequest = new UseSpendingLimitRequest
        {
            Amount = amount,
            ChannelCode = request.ChannelCode.Value,
            CustomerId = customerId
        };

        bool spendingLimitUsed = false;
        bool accountSaleMade = false;
        string? authorizationGuid = null;

        try
        {
            var limitResult =
                await _spendingLimitSaga.ExecuteAsync(limitRequest);

            if (!limitResult.IsSuccess)
            {
                return ServiceResult<SaleResponse>.Failure(
                    limitResult.Error ?? Errors.InsufficientLimitError,
                    limitResult.StatusCode
                );
            }

            spendingLimitUsed = true;

            var accountSaleRequest = new AccountSaleRequest
            {
                AccountNo = accountNo,
                Amount = amount,
                TransactionId = request.TransactionId
            };

            var accountSaleSagaResult = await _accountSaleSaga.ExecuteAsync(accountSaleRequest);
            if (!accountSaleSagaResult.IsSuccess || accountSaleSagaResult.Data == null)
            {
                throw new GeneralException(
                    accountSaleSagaResult.Error ?? Errors.AccountSaleError,
                    accountSaleSagaResult.StatusCode
                );
            }
            
            accountSaleMade = true;

            var authorizationRequest = new CreateAuthorizationRequest
            {
                AccountNo = accountNo,
                Balance = accountSaleSagaResult.Data.Balance,
                CardToken = request.CardNo,
                ChannelCode = request.ChannelCode.Value,
                CustomerId = accountSaleSagaResult.Data.CustomerId,
                Otc = Constants.Otcs.Sale,
                Ots = Constants.Ots.SaleOts.Default,
                TransactionAmount = amount,
                TransactionDescription = "Sale",
                TransactionStatus = "1",
                TransactionId = request.TransactionId
            };

            var authorizationResult =
                await CreateAuthorizationAsync(authorizationRequest);

            if (!authorizationResult.IsSuccess ||
                authorizationResult.Data is null)
            {
                throw new GeneralException(
                    authorizationResult.Error ??
                    Errors.AuthorizationCreateError,
                    authorizationResult.StatusCode
                );
            }

            authorizationGuid = authorizationResult.Data.Guid;
            
            return ServiceResult<SaleResponse>.Success(
                new SaleResponse
                {
                    Balance = accountSaleSagaResult.Data.Balance,
                    TransactionAmount = amount,
                    TransactionTime =
                        authorizationResult.Data.TransactionDate
                }
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Sale failed. Starting compensation. " +
                "AccountNo: {AccountNo}, CardNo: {CardNo}, " +
                "TransactionId: {TransactionId}",
                accountNo,
                request.CardNo,
                request.TransactionId
            );

            await CompensateSaleAsync(
                accountNo,
                amount,
                request.TransactionId,
                limitRequest,
                authorizationGuid,
                accountSaleMade,
                spendingLimitUsed
            );

            throw;
        }
    }
    
    private async Task CompensateSaleAsync(
    string accountNo,
    decimal amount,
    long? transactionId,
    UseSpendingLimitRequest limitRequest,
    string? authorizationGuid,
    bool accountSaleMade,
    bool spendingLimitUsed)
    {
        // 1. Compensate authorization first.
        if (authorizationGuid is not null)
        {
            try
            {
                var authorizationCompensationResult =
                    await AssignStatusAsync(
                        new AssignStatusRequest
                        {
                            Status = "0"
                        } ,
                        authorizationGuid
                    );

                if (!authorizationCompensationResult.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. " +
                        "AuthorizationGuid: {AuthorizationGuid}, " +
                        "TransactionId: {TransactionId}",
                        authorizationGuid,
                        transactionId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. " +
                    "AuthorizationGuid: {AuthorizationGuid}, " +
                    "TransactionId: {TransactionId}",
                    authorizationGuid,
                    transactionId
                );
            }
        }

        // 2. Refund the amount deducted by AccountService.
        if (accountSaleMade)
        {
            try
            {
                var accountCompensationResult =
                    await _accountClient.AccountSaleCompensateAsync(
                        new AccountSaleRequest
                        {
                            AccountNo = accountNo,
                            Amount = amount,
                            TransactionId = transactionId
                        }
                    );

                if (!accountCompensationResult.IsSuccess)
                {
                    _logger.LogError(
                        "Account sale compensation failed. " +
                        "AccountNo: {AccountNo}, " +
                        "TransactionId: {TransactionId}",
                        accountNo,
                        transactionId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Account sale compensation threw an exception. " +
                    "AccountNo: {AccountNo}, " +
                    "TransactionId: {TransactionId}",
                    accountNo,
                    transactionId
                );
            }
        }

        // 3. Restore the spending limit.
        if (spendingLimitUsed)
        {
            try
            {
                await _spendingLimitSaga.CompensateAsync(limitRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Spending limit compensation threw an exception. " +
                    "CustomerId: {CustomerId}, " +
                    "TransactionId: {TransactionId}",
                    limitRequest.CustomerId,
                    transactionId
                );
            }
        }
    }
}