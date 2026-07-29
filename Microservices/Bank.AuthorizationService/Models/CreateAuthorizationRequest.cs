using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models;

public class CreateAuthorizationRequest
{
    [StringLength(
        3,
        ErrorMessage = "Transaction status is too long"
    )]
    public string? TransactionStatus { get; set; }

    [Range(
        1,
        long.MaxValue,
        ErrorMessage = "Customer ID should be greater than 0."
    )]
    public long CustomerId { get; set; }

    [StringLength(
        100,
        ErrorMessage = "Card token is too long."
    )]
    public string? CardToken { get; set; }

    [Range(
        0,
        9999,
        ErrorMessage = "OTC must be in range 0-9999."
    )]
    public int Otc { get; set; }

    [Range(
        0,
        9999,
        ErrorMessage = "OTS must be in range 0-9999."
    )]
    public int Ots { get; set; }

    [StringLength(
        50,
        ErrorMessage = "Transaction description is too long."
    )]
    public string? TransactionDescription { get; set; }

    [StringLength(
        8,
        MinimumLength = 8,
        ErrorMessage = "Account number should be 8 characters length."
    )]
    public string? AccountNo { get; set; }
    
    public ChannelCode ChannelCode { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ErrorMessage = "Balance has an invalid value."
    )]
    public decimal? Balance { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ErrorMessage =
            "Transaction amount has an invalid value."
    )]
    public decimal? TransactionAmount { get; set; }
}