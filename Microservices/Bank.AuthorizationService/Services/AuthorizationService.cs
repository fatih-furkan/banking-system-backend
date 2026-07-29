using Bank.AuthorizationService.Data;
using Bank.AuthorizationService.Models;
using Bank.AuthorizationService.Models.Entities;
using Bank.Shared;
using Microsoft.EntityFrameworkCore;

namespace Bank.AuthorizationService.Services;

public class AuthorizationService
{
    private readonly AppDbContext _context;
    
    public AuthorizationService(AppDbContext context)
    {
        _context = context;
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
            TransactionDescription = createAuthorizationRequest.TransactionDescription
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
            TransactionDescription = auth.TransactionDescription
        };
        return ServiceResult<CreateAuthorizationResponse>.Success(response);
    }
    
}