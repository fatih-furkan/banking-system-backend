namespace Bank.AuthorizationService.Models.Entities;

public class CurrentSpendingLimit
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
    public DateTime LastDailyReset { get; set; }
    public DateTime LastMonthlyReset { get; set; }
    public DateTime LastAnnualReset { get; set; }
}