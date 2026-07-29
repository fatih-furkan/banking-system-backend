namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class AccountSaleResponse
{
    public long TransactionId { get; set; }
    
    public long CustomerId { get; set; }
    
    public decimal Balance { get; set; }
}