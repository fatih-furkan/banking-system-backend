using System.ComponentModel.DataAnnotations;
using Bank.Shared.Constants;

namespace Bank.PointService.Models.Dtos;

public class CreatePointAccountRequest
{
    [Required]
    public long? CustomerId { get; set; }
}