namespace Bank.AuthorizationService.Models.Dtos;

public class UseSpendingLimitResponse
{
    public long CustomerId { get; set; }
    public decimal? TransactionAmount { get; set; }
    public decimal? NewDailyLimit { get; set; }
    public decimal? NewMonthlyLimit { get; set; }
    public decimal? NewAnnualLimit { get; set; }
    public DateTime? TransactionTime { get; set; }
}