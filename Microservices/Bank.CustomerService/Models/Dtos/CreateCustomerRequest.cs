using System.ComponentModel.DataAnnotations;  // logically same with import, data validation. MaxLength and MinLength.

namespace Bank.CustomerService.Models.Dtos;   // address of class.

public class CreateCustomerRequest           // Create customer request body.
{
    [MaxLength(50, ErrorMessage = "Name is too long.")]   // validation for max length.
    [MinLength(1, ErrorMessage = "Name cannot be empty string.")]   // validation for min length.
    public string Name { get; set; } = null!;  // it cannot be null. getter and setter methods are used to get and set the value of the property.

    [MaxLength(50, ErrorMessage = "Surname is too long.")]   // validation for max length.
    [MinLength(1, ErrorMessage = "Surname cannot be empty string." )]  // validation for min length.
    public string Surname { get; set; } = null!;  // it cannot be null. getter and setter methods are used to get and set the value of the property.

    [StringLength(11, MinimumLength = 11, ErrorMessage = "Tc must be 11 characters long.")]  // Tc no must be exactly 11 characters long.
    public string Tc { get; set; } = null!;  // it cannot be null. getter and setter methods are used to get and set the value of the property.

    [MaxLength(2, ErrorMessage = "Status is too long.")]  // validation for max length.
    [MinLength(1, ErrorMessage = "Status cannot be empty string.")] // validation for min length.
    public string Status { get; set; } = null!;   // it cannot be null. getter and setter methods are used to get and set the value of the property.
}