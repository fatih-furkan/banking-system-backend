using System.ComponentModel.DataAnnotations;
using Bank.Shared;

namespace Bank.CardService.Models.Dtos;

public class CreateCardRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [MaxLength(3, ErrorMessage = Constants.ExceptionMessages.BranchCodeLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.BranchCodeEmpty)]
    public string BranchCode { get; set; } = null!;
    public string AccountNo { get; set; } = null!;
}