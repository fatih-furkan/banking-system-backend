namespace Bank.AccountService.Models.Dtos.Account;

public class CreateAccountResponse
{
    public string AccountNo { get; set; } = null!;
    public long CustomerId { get; set; }
    public string BranchCode { get; set; } = null!;
    public string Status { get; set; } = null!;
}