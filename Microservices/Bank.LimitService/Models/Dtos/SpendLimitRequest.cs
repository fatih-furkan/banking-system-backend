using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.LimitService.Models.Dtos;

public class SpendLimitRequest
{
    [Required]
    public decimal? Amount { get; set; }
    
    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}