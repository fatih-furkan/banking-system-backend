using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.AccountService.Models.Dtos;

public class DepositRequest
{
    [Required]
    public decimal? Amount { get; set; }
    public string AccountNo { get; set; } = null!;
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}