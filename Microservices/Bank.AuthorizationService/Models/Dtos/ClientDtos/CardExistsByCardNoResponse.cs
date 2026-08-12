namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class CardExistsByCardNoResponse
{
    public bool Exists { get; set; }
    public string? CardToken { get; set; }
}