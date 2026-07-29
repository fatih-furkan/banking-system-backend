namespace Bank.AccountService.Models.Dtos;

public class DepositResponse
{
    public decimal? TransactionAmount { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? TransactionTime { get; set; }
    
}