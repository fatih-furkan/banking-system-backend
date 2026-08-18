using Bank.TransactionService.Models.Entities;
using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Services;

public interface IPointLogService
{
    Task<PointTransactionLog> LogPointGainAsync(
        long customerId,
        long? campaignId,
        string? transactionId,
        decimal transactionAmount,
        decimal earnedPoint,
        PointTransactionType transactionType = PointTransactionType.Earn,
        string? description = null);

    Task<List<PointTransactionLog>> GetLogsByCustomerIdAsync(long customerId);
}