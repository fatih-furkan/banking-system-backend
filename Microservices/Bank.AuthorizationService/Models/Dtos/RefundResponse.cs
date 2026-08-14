namespace Bank.AuthorizationService.Models.Dtos;

public class RefundResponse
{
    public decimal TransactionAmount { get; set; }
    public decimal Balance { get; set; }
    public DateTime? TransactionTime { get; set; }
}