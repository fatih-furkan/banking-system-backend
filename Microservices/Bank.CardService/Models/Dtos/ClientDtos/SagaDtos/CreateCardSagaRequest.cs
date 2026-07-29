using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;

namespace Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;

public class CreateCardSagaRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [MaxLength(3, ErrorMessage = Constants.ExceptionMessages.BranchCodeLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.BranchCodeEmpty)]
    public string BranchCode { get; set; } = null!;
}