using System.ComponentModel.DataAnnotations;
using Bank.Shared.Enums;

namespace Bank.AccountService.Models.Dtos;

public class WithdrawRequest
{
    [Required]
    public decimal? Amount { get; set; }
    public string AccountNo { get; set; } = null!;
    
    [Required]
    public ChannelCode? ChannelCode { get; set; }
}