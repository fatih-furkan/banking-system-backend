using System.ComponentModel.DataAnnotations;

namespace Bank.LimitService.Models.Dtos;

public class CreateCurrentChargeLimitRequest
{
    [Required]
    public long? CustomerId { get; set; }
}