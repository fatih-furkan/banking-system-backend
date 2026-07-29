
namespace Bank.CardService.Models.Entities;

public class Card
{

    public string CardToken { get; set; }
    public string CardNo { get; set; }
    public string CardAccountNo { get; set; }
    public long CustomerId { get; set; }
}