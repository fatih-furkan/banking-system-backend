namespace Bank.AuthorizationService.Models.Dtos;

public class SaleResponse
{
    public decimal TransactionAmount { get; set; }
    public decimal Balance { get; set; }
    public DateTime? TransactionTime { get; set; }
}