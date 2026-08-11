using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;
using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models.Dtos;

public class SaleRequest
{
    [Required]
    public decimal? Amount { get; set; }
    
    [StringLength(16, MinimumLength = 16)]
    public string CardNo { get; set; } = null!;
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
    
    [StringLength(
        50,
        MinimumLength = 1,
        ErrorMessage = Constants.ExceptionMessages.MerchantNameLengthError
    )]
    public string? MerchantName { get; set; }
    
    [Required]
    public long? TransactionId { get; set; }
}