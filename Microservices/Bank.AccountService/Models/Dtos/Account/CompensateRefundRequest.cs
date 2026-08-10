using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Dtos.Account;

public class CompensateRefundRequest
{
    [Required]
    public Guid? OperationId { get; set; }

    public string AccountNo { get; set; } = null!;

    [Required]
    public decimal? Amount { get; set; }
}