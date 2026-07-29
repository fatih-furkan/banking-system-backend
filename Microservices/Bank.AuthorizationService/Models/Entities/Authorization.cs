using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models.Entities;

public class Authorization
{
    public string? TransactionStatus { get; set; }

    public long CustomerId { get; set; }

    public string? CardToken { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int Otc { get; set; }

    public int Ots { get; set; }

    public string? TransactionDescription { get; set; }

    public string? AccountNo { get; set; }

    public string Guid { get; set; } = null!;

    public ChannelCode ChannelCode { get; set; }

    public decimal? Balance { get; set; }

    public decimal? TransactionAmount { get; set; }
}