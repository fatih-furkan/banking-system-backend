namespace Bank.CardService.Models.Dtos;

public class CreateCardResponse
{
    public string CardToken { get; set; }
    public string CardAccountNo { get; set; }
    public long CustomerId { get; set; }
}