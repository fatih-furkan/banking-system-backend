namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class AddPointResponse
{
    public decimal? Amount { get; set; }

    public string AccountNo { get; set; } = null!;
    
    public long TransactionId { get; set; }
    
    public decimal EarnedPoint { get; set; }
}