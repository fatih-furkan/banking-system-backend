namespace Bank.CardService.Models.Dtos;

public class CheckExistenceByCardNoResponse
{
    public bool Exists { get; set; }
    public string? CardToken { get; set; }
}