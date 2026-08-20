using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;
using Bank.Shared.Enums;

namespace Bank.PointService.Models.ClientModels;

public class CreateAuthorizationRequest
{
    [StringLength(
        3,
        ErrorMessage = Constants.ExceptionMessages.TransactionStatusLong
    )]
    public string? TransactionStatus { get; set; }

    [Required]
    [Range(
        1,
        long.MaxValue,
        ErrorMessage = Constants.ExceptionMessages.CustomerIdShort
    )]
    public long? CustomerId { get; set; }

    [StringLength(
        100,
        ErrorMessage = Constants.ExceptionMessages.CardTokenLong
    )]
    public string? CardToken { get; set; }

    [Range(
        0,
        9999,
        ErrorMessage = Constants.ExceptionMessages.InvalidOtc
    )]
    public int Otc { get; set; }

    [Range(
        0,
        9999,
        ErrorMessage = Constants.ExceptionMessages.InvalidOts
    )]
    public int Ots { get; set; }

    [StringLength(
        50,
        ErrorMessage = Constants.ExceptionMessages.TransactionDescriptionLong
    )]
    public string? TransactionDescription { get; set; }

    [StringLength(
        8,
        MinimumLength = 8,
        ErrorMessage = Constants.ExceptionMessages.AccountNoLengthError
    )]
    public string AccountNo { get; set; } = null!;
    
    public ChannelCode ChannelCode { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ErrorMessage = Constants.ExceptionMessages.InvalidBalance
    )]
    public decimal? Balance { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage = Constants.ExceptionMessages.InvalidTransactionAmount
            
    )]
    public decimal? TransactionAmount { get; set; }
    
    [Required]
    public long? TransactionId { get; set; }
    
    [StringLength(
        50,
        MinimumLength = 1,
        ErrorMessage = Constants.ExceptionMessages.MerchantNameLengthError
    )]
    public string? MerchantName { get; set; }
    
    public long? OriginalTransactionId { get; set; }
    
    public decimal? RefundedAmount { get; set; }
}