namespace Bank.AccountService.Models.Dtos.Account;

public class RefundResponse
{
    public long TransactionId { get; set; }
    
    public decimal Balance { get; set; }
    
    public long CustomerId { get; set; }
}