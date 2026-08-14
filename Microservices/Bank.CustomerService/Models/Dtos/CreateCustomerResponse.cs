// Response DTO for creating a customer in the Bank.CustomerService microservice.


namespace Bank.CustomerService.Models.Dtos; 

public class CreateCustomerResponse  // Müşteri oluşturma isteğinin yanıtını taşıyacak olan yanıt sınıfını tanımlıyor
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Tc { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long CustomerId { get; set; }
}