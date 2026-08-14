using System.ComponentModel.DataAnnotations;

namespace Bank.CustomerService.Models.ClientModels;  //ClientModels 

public class CreateSpendingLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public decimal? DailyLimit { get; set; }
    
    [Required]
    public decimal? MonthlyLimit { get; set; }
    
    [Required]
    public decimal? AnnualLimit { get; set; }
}