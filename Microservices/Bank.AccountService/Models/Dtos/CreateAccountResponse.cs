namespace Bank.AccountService.Models.Dtos;

public class CreateAccountResponse
{
    public string AccountNo { get; set; } = null!;
    public int CustomerId { get; set; }
    public string BranchCode { get; set; } = null!;
    public string Status { get; set; } = null!;
}