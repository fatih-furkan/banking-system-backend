using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Dtos.Limit;

public class CreateChargeLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
}