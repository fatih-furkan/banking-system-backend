using Bank.AuthorizationService.Clients;
using Bank.AuthorizationService.Data;
using Bank.AuthorizationService.Models;
using Bank.AuthorizationService.Models.Dtos;
using Bank.AuthorizationService.Models.Dtos.ClientDtos;
using Bank.AuthorizationService.Models.Entities;
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
    
    public AuthorizationService(AppDbContext context, 
        CardClient cardClient,
        AccountClient accountClient)
    {
        _context = context;
        _cardClient = cardClient;
        _accountClient = accountClient;
    }
    
    public async Task<List<Authorization>> GetAllAuthorizationsAsync()
    {
        return await _context.Authorizations.ToListAsync();
    }

    public async Task<ServiceResult<CreateAuthorizationResponse>> CreateAuthorizationAsync(
        CreateAuthorizationRequest createAuthorizationRequest)
    {
        Authorization auth = new Authorization
        {
            Balance = createAuthorizationRequest.Balance,
            AccountNo = createAuthorizationRequest.AccountNo,
            CardToken = createAuthorizationRequest.CardToken,
            ChannelCode = createAuthorizationRequest.ChannelCode,
            CustomerId = createAuthorizationRequest.CustomerId,
            Guid = Guid.NewGuid().ToString(),
            Otc = createAuthorizationRequest.Otc,
            Ots = createAuthorizationRequest.Ots,
            TransactionAmount = createAuthorizationRequest.TransactionAmount,
            TransactionDate = DateTime.UtcNow,
            TransactionStatus = createAuthorizationRequest.TransactionStatus,
            TransactionDescription = createAuthorizationRequest.TransactionDescription,
            TransactionId = createAuthorizationRequest.TransactionId
        };
        
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
            TransactionId = auth.TransactionId!.Value
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

    public async Task<ServiceResult<SaleResponse>> SaleAsync(SaleRequest request)
    {
        if (decimal.Round(request.Amount!.Value, 2) != request.Amount)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.PrecisionError, 403);
        }
        
        if (request.Amount < 0)
        {
            return ServiceResult<SaleResponse>.Failure(
            Errors.NegativeAmountError, 403);
        }

        if (request.ChannelCode != ChannelCode.Fast
            && request.ChannelCode != ChannelCode.Online
            && request.ChannelCode != ChannelCode.Pos)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.UnauthorizedChannelError, 403);
        }
        var accountNoResult = await _cardClient.FindAccountNoByCardNoAsync(request.CardNo);
        if (!accountNoResult.IsSuccess || accountNoResult.Data == null)
        {
            return ServiceResult<SaleResponse>.Failure(
                Errors.AccountNotFoundError, 403);
        }

        string accountNo = accountNoResult.Data;

        
        var accountSaleResult = await _accountClient.AccountSaleAsync(new AccountSaleRequest
        {
            AccountNo = accountNo,
            Amount = request.Amount,
            TransactionId = request.TransactionId
        });

        if (!accountSaleResult.IsSuccess || accountSaleResult.Data == null)
        {
            //todo compensate eklenebilir mi?
            
            return ServiceResult<SaleResponse>.Failure(
                Errors.AccountSaleError, 403);
        }
        
        var authRequest = new CreateAuthorizationRequest
        {
            AccountNo = accountNo,
            Balance = accountSaleResult.Data.Balance,
            CardToken = null,
            ChannelCode = request.ChannelCode!.Value,
            CustomerId = accountSaleResult.Data.CustomerId,
            Otc = Constants.Otcs.Sale,
            Ots = Constants.Ots.SaleOts.Default,
            TransactionAmount = request.Amount,
            TransactionDescription = "sale",
            TransactionStatus = "1",
            TransactionId = request.TransactionId
        };

        var createAuthorizationResult = await CreateAuthorizationAsync(authRequest);
        if (!createAuthorizationResult.IsSuccess || createAuthorizationResult.Data == null)
        {
            //todo compensate
        }

        return ServiceResult<SaleResponse>.Success(new SaleResponse
        {
            Balance = accountSaleResult.Data.Balance,
            TransactionAmount = request.Amount.Value,
            TransactionTime = DateTime.UtcNow
        });
    }
    
}