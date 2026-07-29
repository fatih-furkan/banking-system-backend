namespace Bank.CardService.Models.Dtos;

public class CreateCardRequest
{
    public long customerId { get; set; }
    public string branchCode { get; set; } = null!;
}