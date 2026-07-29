using System.ComponentModel.DataAnnotations;

namespace Bank.AuthorizationService.Models;

public class AssignStatusRequest
{
    [MaxLength(2, ErrorMessage = "Status is too long")]
    [MinLength(1, ErrorMessage = "Status cannot be empty")]
    public string Status { get; set; } = null!;
}