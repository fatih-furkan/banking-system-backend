using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models.Dtos;

public class UseSpendingLimitRequest
{
    [Required]
    public decimal? Amount { get; set; }
    
    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}