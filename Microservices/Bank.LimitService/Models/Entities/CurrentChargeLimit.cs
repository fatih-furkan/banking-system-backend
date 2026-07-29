namespace Bank.LimitService.Models.Entities;

public class CurrentChargeLimit
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
    public DateTime LastDailyReset { get; set; }
    public DateTime LastMonthlyReset { get; set; }
    public DateTime LastAnnualReset { get; set; }
}