namespace Bank.CardService.Models.Dtos;

public class CreateCardRequest
{
    public long CustomerId { get; set; }
    public string BranchCode { get; set; } = null!;
    public string AccountNo { get; set; } = null!;
}