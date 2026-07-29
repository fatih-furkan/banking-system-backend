using System.ComponentModel.DataAnnotations;

namespace Bank.LimitService.Models.Dtos;

public class CreateChargeLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
}