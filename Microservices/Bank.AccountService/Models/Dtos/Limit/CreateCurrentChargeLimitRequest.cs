using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Dtos.Limit;

public class CreateCurrentChargeLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
}