namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class CompensateAccountSaleResponse
{
    public decimal Balance { get; set; }
    
    public long CustomerId { get; set; }
}