namespace Bank.AuthorizationService.Models.Dtos;

public class CompensateUseSpendingLimitResponse
{
    public long CustomerId { get; set; }
    public decimal? TransactionAmount { get; set; }
    public DateTime? TransactionTime { get; set; }
}