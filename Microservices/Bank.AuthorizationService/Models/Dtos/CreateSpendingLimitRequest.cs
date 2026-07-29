using System.ComponentModel.DataAnnotations;

namespace Bank.AuthorizationService.Models.Dtos;

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