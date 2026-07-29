namespace Bank.AccountService.Models.Dtos.Limit;

public class CreateCurrentChargeLimitResponse
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
}