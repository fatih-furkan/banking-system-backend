using System.ComponentModel.DataAnnotations;  // logically same with import, data validation. MaxLength and MinLength. 

namespace Bank.CustomerService.Models.Dtos;  // address of class.

public class AssignStatusRequest  // Request Body.
{
    [MaxLength(2, ErrorMessage = "Status is too long")]   // validation for max length.
    [MinLength(1, ErrorMessage = "Status cannot be empty")]  // validation for min length.
    public string Status { get; set; } = null!;   // it cannot be null. getter and setter methods are used to get and set the value of the property.
}