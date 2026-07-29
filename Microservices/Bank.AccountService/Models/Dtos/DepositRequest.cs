using Bank.Shared.Enums;

namespace Bank.AccountService.Models.Dtos;

public class DepositRequest
{
    public decimal Amount { get; set; }
    public string AccountNo { get; set; } = null!;
    public ChannelCode ChannelCode { get; set; }
}