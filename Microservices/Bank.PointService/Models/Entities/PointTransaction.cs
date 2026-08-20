using System;

namespace Bank.PointService.Models.Entities;

public class PointTransaction
{
    public long TransactionLogId { get; set; }
    public string AccountNo { get; set; } = null!;
    public long CustomerId { get; set; }
    public long? SourceTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = null!; 
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}