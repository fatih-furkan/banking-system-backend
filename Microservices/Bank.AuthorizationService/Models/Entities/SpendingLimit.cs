namespace Bank.AuthorizationService.Models.Entities;

public class SpendingLimit
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
}