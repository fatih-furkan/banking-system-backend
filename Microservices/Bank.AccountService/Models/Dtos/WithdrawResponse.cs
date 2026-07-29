namespace Bank.AccountService.Models.Dtos;

public class WithdrawResponse
{
    public decimal? TransactionAmount { get; set; }
    public decimal? Balance { get; set; }
    public DateTime? TransactionTime { get; set; }

}