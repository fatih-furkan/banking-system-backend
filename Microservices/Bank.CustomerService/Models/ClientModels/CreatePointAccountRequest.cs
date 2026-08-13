using System.ComponentModel.DataAnnotations;

namespace Bank.CustomerService.Models.ClientModels;

public class CreatePointAccountRequest
{
    [Required]
    public long? CustomerId { get; set; }
}