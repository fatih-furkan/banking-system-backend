using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.AccountService.Models.Dtos.Limit;

public class UseChargeLimitRequest
{
    [Required]
    public decimal? Amount { get; set; }
    
    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}