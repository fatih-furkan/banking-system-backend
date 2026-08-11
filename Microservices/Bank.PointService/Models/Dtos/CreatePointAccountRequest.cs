using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;

namespace Bank.PointService.Models.Dtos;

public class CreatePointAccountRequest
{
    [Required]
    public long? CustomerId { get; set; }
    
    [MaxLength(2, ErrorMessage = Constants.ExceptionMessages.StatusLong)]
    [MinLength(1, ErrorMessage = Constants.ExceptionMessages.StatusEmpty)]
    public string Status { get; set; } = null!;
}