using System.ComponentModel.DataAnnotations;  // logically same with import, data validation. MaxLength and MinLength.

namespace Bank.CustomerService.Models.Dtos;   // address of class.

public class CreateCustomerRequest           // Create customer request body.
{
    [MaxLength(50, ErrorMessage = "Name is too long.")]
    [MinLength(1, ErrorMessage = "Name cannot be empty string.")]
    public string Name { get; set; } = null!;
    
    [MaxLength(50, ErrorMessage = "Surname is too long.")]
    [MinLength(1, ErrorMessage = "Surname cannot be empty string." )]
    public string Surname { get; set; } = null!;
    
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Tc must be 11 characters long.")]
    public string Tc { get; set; } = null!;
}