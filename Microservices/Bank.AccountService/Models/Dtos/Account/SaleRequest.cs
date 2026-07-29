using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Dtos.Account;

public class SaleRequest
{
    [Required]
    public decimal? Amount { get; set; }
    
    [StringLength(8, MinimumLength = 8)]
    public string AccountNo { get; set; } = null!;
    
    [Required]
    public long? TransactionId { get; set; }
}