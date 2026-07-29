using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Entities.Limit;

public class AccountConsumptionLimit
{
    [Required]
    public string AccountNo { get; set; } = null!;
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public decimal AnnualLimit { get; set; }
}