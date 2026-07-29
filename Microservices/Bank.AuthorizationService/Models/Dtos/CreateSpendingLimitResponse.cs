namespace Bank.AuthorizationService.Models.Dtos;

public class CreateSpendingLimitResponse
{
    public long CustomerId { get; set; }
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
    
    public decimal CurrentDailyLimit { get; set; }
    public decimal CurrentMonthlyLimit { get; set; }
    public decimal CurrentAnnualLimit { get; set; }
}