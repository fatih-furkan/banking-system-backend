namespace Bank.AccountService.Models.Dtos.Account;

public class CompensateSaleResponse
{
    public decimal Balance { get; set; }
    
    public long CustomerId { get; set; }
}