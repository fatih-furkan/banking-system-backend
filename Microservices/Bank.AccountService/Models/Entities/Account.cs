namespace Bank.AccountService.Models.Entities;

public class Account
{
    public string AccountNo { get; set; }
    public string? CardToken { get; set; }
    public int CustomerId { get; set; }
    public string BranchCode { get; set; }
    public string Status { get; set; }
    public decimal Balance { get; set; }
}