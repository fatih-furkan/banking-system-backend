using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.PointService.Models.Dtos;

public class AddPointRequest
{
    [Required]
    public decimal? Amount { get; set; }

    [Required]
    public long? CustomerId { get; set; }
    
    [Required]
    public long? TransactionId { get; set; }

    [Required] public string CardNo { get; set; } = null!;
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}