using System.ComponentModel.DataAnnotations;
using Bank.Shared;
using Bank.Shared.Constants;

namespace Bank.AccountService.Models.Dtos;

public class CreateAccountRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [MaxLength(3, ErrorMessage = Constants.ExceptionMessages.BranchCodeLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.BranchCodeEmpty)]
    public string BranchCode { get; set; } = null!;
    
    [MaxLength(2, ErrorMessage = Constants.ExceptionMessages.StatusLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.StatusEmpty)]
    public string Status { get; set; } = null!; //Varsa 1 yoksa  0 gibi

}