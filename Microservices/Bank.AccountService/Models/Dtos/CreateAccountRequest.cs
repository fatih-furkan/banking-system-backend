using System.ComponentModel.DataAnnotations;

namespace Bank.AccountService.Models.Dtos;

public class CreateAccountRequest
{
    public int CustomerId { get; set; }
    
    [MaxLength(3, ErrorMessage = "Branch code is too long")]
    [MinLength(1, ErrorMessage = "Branch code cannot be empty")]
    public string BranchCode { get; set; } = null!;
    
    [MaxLength(2, ErrorMessage = "Status is too long")]
    [MinLength(1, ErrorMessage = "Status cannot be empty")]
    public string Status { get; set; } = null!; //Varsa 1 yoksa  0 gibi

}