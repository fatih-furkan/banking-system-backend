namespace Bank.CustomerService.Models.Dtos;

public class CreateCustomerResponse
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Tc { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long CustomerId { get; set; }
}