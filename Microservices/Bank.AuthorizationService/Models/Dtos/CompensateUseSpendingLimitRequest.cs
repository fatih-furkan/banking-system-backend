using System.ComponentModel.DataAnnotations;

namespace Bank.AuthorizationService.Models.Dtos;

public class CompensateUseSpendingLimitRequest
{
    
    [Required]
    public Guid? OperationId { get; set; }

    public long CustomerId { get; set; }

    [Required]
    public decimal? Amount { get; set; }
}