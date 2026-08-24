using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;

namespace Bank.AccountService.Models.Dtos.Account;

public class CreateAccountRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [MaxLength(3, ErrorMessage = Constants.ExceptionMessages.BranchCodeLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.BranchCodeEmpty)]
    public string BranchCode { get; set; } = null!;
}