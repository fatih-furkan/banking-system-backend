namespace Bank.PointService.Models.ClientModels;

public class CardExistsByCardNoResponse
{
    public bool Exists { get; set; }
    public string? CardToken { get; set; }
}