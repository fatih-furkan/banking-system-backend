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
using Oracle.ManagedDataAccess.Client;

namespace Bank.AuthorizationService.Services;

public class AuthorizationService
{
    private readonly AppDbContext _context;
    private readonly CardClient _cardClient;
    private readonly AccountClient _accountClient;
    private readonly CustomerClient _customerClient;
    private readonly PointClient _pointClient;
    private readonly CampaignClient _campaignClient;
    private readonly SpendingLimitSaga _spendingLimitSaga;
    private readonly AccountSaleSaga _accountSaleSaga;
    private readonly AccountRefundSaga _accountRefundSaga;
    private readonly ILogger<AuthorizationService> _logger;
    
    public AuthorizationService(AppDbContext context, 
        CardClient cardClient,
        AccountClient accountClient,
        SpendingLimitSaga spendingLimitSaga,
        AccountSaleSaga accountSaleSaga,
        AccountRefundSaga accountRefundSaga,
        ILogger<AuthorizationService> logger,
        CustomerClient customerClient,
        PointClient pointClient,
        CampaignClient campaignClient)
    {
        _context = context;
        _cardClient = cardClient;
        _accountClient = accountClient;
        _customerClient = customerClient;
        _pointClient = pointClient;
        _campaignClient = campaignClient;
        _spendingLimitSaga = spendingLimitSaga;
        _accountSaleSaga = accountSaleSaga;
        _accountRefundSaga = accountRefundSaga;
        _logger = logger;
    }
    
    public async Task<List<Authorization>> GetAllAuthorizationsAsync()
    {
        return await _context.Authorizations.ToListAsync();
    }

    public async Task<ServiceResult<CreateAuthorizationResponse>> CreateAuthorizationAsync(
        CreateAuthorizationRequest request)
    {
        //account existence
        var accountExistsResult = await _accountClient.AccountExistsAsync(request.AccountNo);
        if (!accountExistsResult.IsSuccess)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(accountExistsResult.Error ?? Errors.AccountClientError,
                    accountExistsResult.StatusCode);
        }
        
        if (accountExistsResult.Data == false)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(Errors.AccountNotFoundError);
        }

        //card existence
        if (request.CardToken != null)
        {
            var cardExistsResult = await _cardClient.CardExistsAsync(request.CardToken);
            if (!cardExistsResult.IsSuccess)
            {
                return ServiceResult<CreateAuthorizationResponse>
                    .Failure(cardExistsResult.Error ?? Errors.CardClientError,
                        cardExistsResult.StatusCode);
            }
        
            if (cardExistsResult.Data == false)
            {
                return ServiceResult<CreateAuthorizationResponse>
                    .Failure(Errors.CardNotFoundError);
            }
        }
        
        //customer existence
        var customerExistsResult = await _customerClient.CustomerExistsAsync(request.CustomerId!.Value);
        if (!customerExistsResult.IsSuccess)
        {
            return ServiceResult<CreateAuthorizationResponse>
                .Failure(customerExistsResult.Error ?? Errors.CustomerClientError,
                    customerExistsResult.StatusCode);
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
            TransactionId = request.TransactionId!.Value,
            MerchantName = request.MerchantName,
            RefundedAmount = request.RefundedAmount,
            OriginalTransactionId = request.OriginalTransactionId
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
        
        try
        {
            _context.Authorizations.Add(auth);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (IsDuplicateTransactionId(exception))
        {
            return ServiceResult<CreateAuthorizationResponse>.Failure(
                Errors.TransactionAlreadyExistsError,
                StatusCodes.Status409Conflict
            );
        }
        
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
            TransactionId = auth.TransactionId,
            MerchantName = auth.MerchantName
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
        decimal amount = request.Amount!.Value;

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

        if (request.ChannelCode != ChannelCode.Online &&
            request.ChannelCode != ChannelCode.Pos)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.UnauthorizedChannelError,
                StatusCodes.Status403Forbidden
            );
        }

        var cardExistsResult = await _cardClient.CardExistsByCardNoAsync(request.CardNo);
        
        if (!cardExistsResult.IsSuccess ||
            string.IsNullOrWhiteSpace(cardExistsResult.Data?.CardToken))
        {
            return ServiceResult<SaleResponse>.Failure(
                cardExistsResult.Error ?? Errors.AccountNotFoundError,
                cardExistsResult.StatusCode
            );
        }

        var cardToken = cardExistsResult.Data.CardToken;

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

        Guid operationId = Guid.NewGuid();
        
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
                ChannelCode = request.ChannelCode.Value,
                CustomerId = accountSaleSagaResult.Data.CustomerId,
                Otc = Constants.Otcs.Sale,
                Ots = Constants.Ots.SaleOts.Default,
                TransactionAmount = amount,
                TransactionDescription = "Sale",
                TransactionStatus = "1",
                TransactionId = request.TransactionId,
                MerchantName = request.MerchantName,
                CardToken = cardToken,
                RefundedAmount = 0
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

            //errors in point section should not affect the sale process.
            try
            {
                var getCampaignsResult = await _campaignClient
                    .GetCampaigns(status: "1", targetDate: DateTime.UtcNow);
                if (!getCampaignsResult.IsSuccess)
                {
                    //todo log - campaigns could not be obtained
                }

                else
                {
                    foreach (Campaign campaign in getCampaignsResult.Data!)
                    {
                        foreach (CampaignCriterion criterion in campaign.Criteria)
                        {
                            if (criterion.MinAmount <= request.Amount &&
                                criterion.MaxAmount >= request.Amount)
                            {
                                if (criterion.RewardCalculationType == RewardCalculationType.Fixed)
                                {
                                    var addPointResult = await _pointClient.AddPointAsync(
                                        new AddPointRequest
                                        {
                                            Amount = criterion.RewardValue,
                                            CustomerId = accountSaleSagaResult.Data.CustomerId,
                                            TransactionId = request.TransactionId,
                                            CardNo = request.CardNo,
                                            ChannelCode = request.ChannelCode
                                        }
                                    );
                                }

                                else if (criterion.RewardCalculationType == RewardCalculationType.Percentage)
                                {
                                    var addPointResult = await _pointClient.AddPointAsync(
                                        new AddPointRequest
                                        {
                                            Amount = decimal.Round(
                                                request.Amount.Value * criterion.RewardValue / 100m,
                                                2,
                                                MidpointRounding.AwayFromZero
                                            ),
                                            CustomerId = accountSaleSagaResult.Data.CustomerId,
                                            TransactionId = request.TransactionId,
                                            CardNo = request.CardNo,
                                            ChannelCode = request.ChannelCode
                                        }
                                    );
                                }

                                //todo log
                            }
                        }
                    }
                }
            }
            catch
            {
                //todo log
            }
            
            
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

            var compensateLimitRequest = new CompensateUseSpendingLimitRequest
            {
                OperationId = operationId,
                CustomerId = customerId,
                Amount = amount
            };
            
            await CompensateSaleAsync(
                accountNo,
                amount,
                compensateLimitRequest,
                authorizationGuid,
                accountSaleMade,
                spendingLimitUsed,
                operationId
            );

            throw;
        }
    }
    
    private async Task CompensateSaleAsync(
    string accountNo,
    decimal amount,
    CompensateUseSpendingLimitRequest limitRequest,
    string? authorizationGuid,
    bool accountSaleMade,
    bool spendingLimitUsed,
    Guid operationId)
    {
       // Reverse order of the original operations.

        // 1. Cancel authorization
        if (authorizationGuid is not null)
        {
            try
            {
                var result = await AssignStatusAsync(
                    new AssignStatusRequest
                    {
                        Status = "0"
                    },
                    authorizationGuid
                );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Authorization compensation failed. " +
                        "OperationId: {OperationId}, AuthorizationGuid: {AuthorizationGuid}",
                        operationId,
                        authorizationGuid
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation threw an exception. " +
                    "OperationId: {OperationId}, AuthorizationGuid: {AuthorizationGuid}",
                    operationId,
                    authorizationGuid
                );
            }
        }

        // 2. Restore the balance in AccountService. .
        if (accountSaleMade)
        {
            try
            {
                var result =
                    await _accountClient.AccountSaleCompensateAsync(
                        new CompensateAccountSaleRequest
                        {
                            OperationId = operationId,
                            AccountNo = accountNo,
                            Amount = amount
                        }
                    );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Account sale compensation failed. " +
                        "OperationId: {OperationId}, AccountNo: {AccountNo}",
                        operationId,
                        accountNo
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Account sale compensation threw an exception. " +
                    "OperationId: {OperationId}, AccountNo: {AccountNo}",
                    operationId,
                    accountNo
                );
            }
        }

        // 3. Restore the spending limit
        if (spendingLimitUsed)
        {
            try
            {
                var result = await _spendingLimitSaga.CompensateAsync(
                    new CompensateUseSpendingLimitRequest
                    {
                        OperationId = operationId,
                        CustomerId = limitRequest.CustomerId,
                        Amount = limitRequest.Amount
                    }
                );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Spending limit compensation failed. " +
                        "OperationId: {OperationId}, CustomerId: {CustomerId}",
                        operationId,
                        limitRequest.CustomerId
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Spending limit compensation threw an exception. " +
                    "OperationId: {OperationId}, CustomerId: {CustomerId}",
                    operationId,
                    limitRequest.CustomerId
                );
            }
        }
    }
    
    public async Task<ServiceResult<RefundResponse>> RefundAsync(
    RefundRequest request)
    {
        decimal amount = request.Amount!.Value;

        if (decimal.Round(amount, 2) != amount)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.PrecisionError,
                StatusCodes.Status400BadRequest
            );
        }

        if (amount <= 0)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.NegativeAmountError,
                StatusCodes.Status400BadRequest
            );
        }
        
        Authorization? theAuthorization = await _context.Authorizations
            .SingleOrDefaultAsync(a => a.TransactionId == request.SaleTransactionId);

        if (theAuthorization == null)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.TransactionNotExistError,
                StatusCodes.Status400BadRequest
            );
        }
        
        if (request.RefundType == RefundType.Complete)
        {
            if (theAuthorization.TransactionAmount != amount)
            {
                return ServiceResult<RefundResponse>.Failure(
                    Errors.AmountRefundTypeMismatchError);
            }
        }

        else
        {
            if (theAuthorization.TransactionAmount <= amount)
            {
                return ServiceResult<RefundResponse>.Failure(
                    Errors.AmountRefundTypeMismatchError);
            }
        }

        if (theAuthorization.MerchantName != request.MerchantName)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.MerchantNameMismatchError);
        }
        
        if (request.ChannelCode != ChannelCode.Online &&
            request.ChannelCode != ChannelCode.Pos)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.UnauthorizedChannelError,
                StatusCodes.Status403Forbidden
            );
        }
        
        
        var cardExistsResult = await _cardClient.CardExistsByCardNoAsync(request.CardNo);
        
        if (!cardExistsResult.IsSuccess ||
            string.IsNullOrWhiteSpace(cardExistsResult.Data?.CardToken))
        {
            return ServiceResult<RefundResponse>.Failure(
                cardExistsResult.Error ?? Errors.AccountNotFoundError,
                cardExistsResult.StatusCode
            );
        }

        var cardToken = cardExistsResult.Data.CardToken;

        if (theAuthorization.CardToken != cardToken)
        {
            return ServiceResult<RefundResponse>.Failure(
                Errors.CardTokenMismatchError);
        }
        
        var accountNoResult =
            await _cardClient.FindAccountNoByCardNoAsync(request.CardNo);

        if (!accountNoResult.IsSuccess ||
            string.IsNullOrWhiteSpace(accountNoResult.Data))
        {
            return ServiceResult<RefundResponse>.Failure(
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
            return ServiceResult<RefundResponse>.Failure(
                customerIdResult.Error ?? Errors.CustomerNotFoundError,
                customerIdResult.StatusCode
            );
        }

        long customerId = customerIdResult.Data.Value;
        
        bool accountRefundMade = false;
        string? authorizationGuid = null;

        Guid operationId = Guid.NewGuid();
        
        try
        {

            var accountRefundRequest = new AccountRefundRequest
            {
                AccountNo = accountNo,
                Amount = amount,
                TransactionId = request.TransactionId
            };

            var accountRefundSagaResult = await _accountRefundSaga.ExecuteAsync(accountRefundRequest);
            if (!accountRefundSagaResult.IsSuccess || accountRefundSagaResult.Data == null)
            {
                throw new GeneralException(
                    accountRefundSagaResult.Error ?? Errors.AccountSaleError,
                    accountRefundSagaResult.StatusCode
                );
            }
            
            accountRefundMade = true;

            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            
            int affectedRefundedAmountRows = await _context.Authorizations
                .Where(x =>
                    x.TransactionId == request.SaleTransactionId &&
                    x.TransactionStatus == "1" &&
                    (x.RefundedAmount ?? 0m) + amount <= x.TransactionAmount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.RefundedAmount,
                        x => x.RefundedAmount + amount
                    )
                );

            if (affectedRefundedAmountRows == 0)
            {
                throw new GeneralException(
                    Errors.RefundedAmountUpdateError,
                    StatusCodes.Status409Conflict
                );
            }
            
            await _context.Authorizations
                .Where(authorization =>
                    authorization.TransactionId == request.SaleTransactionId &&
                    authorization.TransactionStatus == "1" &&
                    authorization.RefundedAmount == authorization.TransactionAmount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        authorization => authorization.TransactionStatus,
                        "0"
                    )
                );
            
            var authorizationRequest = new CreateAuthorizationRequest
            {
                AccountNo = accountNo,
                Balance = accountRefundSagaResult.Data.Balance,
                ChannelCode = request.ChannelCode.Value,
                CustomerId = accountRefundSagaResult.Data.CustomerId,
                Otc = Constants.Otcs.Refund,
                Ots = request.RefundType == RefundType.Complete ? 
                    Constants.Ots.RefundOts.Complete : Constants.Ots.RefundOts.Partial,
                TransactionAmount = amount,
                TransactionDescription = "Refund",
                TransactionStatus = "1",
                TransactionId = request.TransactionId,
                MerchantName = request.MerchantName,
                CardToken = cardToken,
                OriginalTransactionId = request.SaleTransactionId
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
            
            await transaction.CommitAsync();

            authorizationGuid = authorizationResult.Data.Guid;
            
            return ServiceResult<RefundResponse>.Success(
                new RefundResponse
                {
                    Balance = accountRefundSagaResult.Data.Balance,
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
                "Refund failed. Starting compensation. " +
                "AccountNo: {AccountNo}, CardNo: {CardNo}, " +
                "TransactionId: {TransactionId}",
                accountNo,
                request.CardNo,
                request.TransactionId
            );
            
            await CompensateRefundAsync(
                accountNo,
                amount,
                authorizationGuid,
                accountRefundMade,
                operationId,
                request.SaleTransactionId!.Value
            );

            throw;
        }
    }

    private async Task CompensateRefundAsync(
        string accountNo,
        decimal amount,
        string? authorizationGuid,
        bool accountRefundMade,
        Guid operationId,
        long saleTransactionId)
    {
        if (authorizationGuid is not null)
        {
            try
            {
                const string operationType =
                    Constants.CompensationOperationTypes
                        .CompensateRefundAuthorization;

                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                bool alreadyCompleted =
                    await _context.CompletedSagaOperations
                        .AnyAsync(x =>
                            x.OperationId == operationId &&
                            x.OperationType == operationType
                        );

                if (alreadyCompleted)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    // 1. Cancel the newly created refund authorization
                    var newEntryResult = await AssignStatusAsync(
                        new AssignStatusRequest
                        {
                            Status = "0"
                        },
                        authorizationGuid
                    );

                    if (!newEntryResult.IsSuccess)
                    {
                        throw new GeneralException(
                            newEntryResult.Error ??
                            Errors.AuthCompensateError,
                            newEntryResult.StatusCode
                        );
                    }

                    // 2. Undo RefundedAmount increase
                    int affectedRefundedAmountRows =
                        await _context.Authorizations
                            .Where(x =>
                                x.TransactionId == saleTransactionId &&
                                x.RefundedAmount >= amount)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(
                                    x => x.RefundedAmount,
                                    x => x.RefundedAmount - amount
                                )
                            );

                    if (affectedRefundedAmountRows == 0)
                    {
                        throw new GeneralException(
                            Errors.AuthCompensateError,
                            StatusCodes.Status500InternalServerError
                        );
                    }

                    // 3. Make the original sale refundable/active again
                    var oldEntryResult =
                        await AssignStatusWithTrxnIdAsync(
                            new AssignStatusRequest
                            {
                                Status = "1"
                            },
                            saleTransactionId
                        );

                    if (!oldEntryResult.IsSuccess)
                    {
                        throw new GeneralException(
                            oldEntryResult.Error ??
                            Errors.AuthCompensateError,
                            oldEntryResult.StatusCode
                        );
                    }

                    // 4. Record successful compensation
                    _context.CompletedSagaOperations.Add(
                        new CompletedSagaOperation
                        {
                            OperationId = operationId,
                            OperationType = operationType,
                            CompletedAt = DateTime.UtcNow
                        }
                    );

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Authorization compensation failed. " +
                    "OperationId: {OperationId}, AuthorizationGuid: {AuthorizationGuid}",
                    operationId,
                    authorizationGuid
                );
            }
        }

        // 2. Restore the balance in AccountService.
        if (accountRefundMade)
        {
            try
            {
                var result =
                    await _accountClient.AccountRefundCompensateAsync(
                        new CompensateAccountRefundRequest
                        {
                            OperationId = operationId,
                            AccountNo = accountNo,
                            Amount = amount
                        }
                    );

                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Account sale compensation failed. " +
                        "OperationId: {OperationId}, AccountNo: {AccountNo}",
                        operationId,
                        accountNo
                    );
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Account sale compensation threw an exception. " +
                    "OperationId: {OperationId}, AccountNo: {AccountNo}",
                    operationId,
                    accountNo
                );
            }
        }
    }
    
    private static bool IsDuplicateTransactionId(
        DbUpdateException exception)
    {
        OracleException? oracleException =
            FindOracleException(exception);

        return oracleException is not null
               && oracleException.Number == 1
               && oracleException.Message.Contains(
                   "IX_AUTHORIZATION_TRXN_ID",
                   StringComparison.OrdinalIgnoreCase
               );
    }
    
    private static OracleException? FindOracleException(
        Exception exception)
    {
        Exception? currentException = exception;

        while (currentException is not null)
        {
            if (currentException is OracleException oracleException)
            {
                return oracleException;
            }

            currentException = currentException.InnerException;
        }

        return null;
    }
    
    private async Task<ServiceResult<Unit>> AssignStatusWithTrxnIdAsync(AssignStatusRequest request, long transactionId)
    {
        bool isOnlyDigits =
            !string.IsNullOrEmpty(request.Status) &&
            request.Status.All(c => c is >= '0' and <= '9');
        
        if (!isOnlyDigits)
        {
            return ServiceResult<Unit>.Failure(Errors.InvalidStatusError);
        }
        
        var auth = await _context.Authorizations
            .SingleOrDefaultAsync(auth => auth.TransactionId == transactionId);
        
        if (auth == null)
        {
            return ServiceResult<Unit>.Failure(Errors.AuthorizationNotFoundError);
        }
        
        auth.TransactionStatus = request.Status;
        await _context.SaveChangesAsync();
        return ServiceResult<Unit>.Success(new Unit());
    }
}