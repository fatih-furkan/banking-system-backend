using System.ComponentModel.DataAnnotations;

namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class CompensateAccountRefundRequest
{
    [Required]
    public Guid? OperationId { get; set; }

    public string AccountNo { get; set; } = null!;

    [Required]
    public decimal? Amount { get; set; }
}