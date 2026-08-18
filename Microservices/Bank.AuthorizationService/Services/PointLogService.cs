using Bank.AuthorizationService.Data;
using Bank.TransactionService.Models.Entities;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.AuthorizationService.Services;

public class PointLogService : IPointLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PointLogService> _logger;

    public PointLogService(AppDbContext context, ILogger<PointLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PointTransactionLog> LogPointGainAsync(
        long customerId,
        long? campaignId,
        string? transactionId,
        decimal transactionAmount,
        decimal earnedPoint,
        PointTransactionType transactionType = PointTransactionType.Earn,
        string? description = null)
    {
        var log = new PointTransactionLog
        {
            CustomerId = customerId,
            CampaignId = campaignId,
            TransactionId = transactionId,
            TransactionAmount = transactionAmount,
            EarnedPoint = earnedPoint,
            TransactionType = transactionType,
            TransactionDate = DateTime.UtcNow,
            Description = description ?? $"Point Transaction: {earnedPoint} Points"
        };

        await _context.PointTransactionLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Point transaction logged. LogId: {LogId}, CustomerId: {CustomerId}, EarnedPoints: {EarnedPoint}",
            log.LogId, customerId, earnedPoint);

        return log;
    }

    public async Task<List<PointTransactionLog>> GetLogsByCustomerIdAsync(long customerId)
    {
        return await _context.PointTransactionLogs
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
    }
}