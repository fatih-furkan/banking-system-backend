namespace Bank.AccountService.Models.Dtos.Limit;

public class CompensateUseChargeLimitResponse
{
    public long CustomerId { get; set; }
    public decimal? TransactionAmount { get; set; }
    public DateTime? TransactionTime { get; set; }
}