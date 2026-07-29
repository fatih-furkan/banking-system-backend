namespace Bank.AccountService.Models.Dtos.Account;

public class DepositResponse
{
    public decimal? TransactionAmount { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? TransactionTime { get; set; }
    public long TransactionId { get; set; }
}