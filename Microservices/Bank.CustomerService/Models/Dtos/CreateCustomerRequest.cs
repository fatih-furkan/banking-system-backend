using System.ComponentModel.DataAnnotations; 

namespace Bank.CustomerService.Models.Dtos;   

public class CreateCustomerRequest          
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