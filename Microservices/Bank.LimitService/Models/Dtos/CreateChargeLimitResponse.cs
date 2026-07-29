namespace Bank.LimitService.Models.Dtos;

public class CreateChargeLimitResponse
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
}