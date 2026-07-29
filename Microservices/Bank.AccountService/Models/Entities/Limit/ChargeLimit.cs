namespace Bank.AccountService.Models.Entities.Limit;

public class ChargeLimit
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
}