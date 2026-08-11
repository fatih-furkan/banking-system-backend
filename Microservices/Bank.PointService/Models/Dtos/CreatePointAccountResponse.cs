namespace Bank.PointService.Models.Dtos;

public class CreatePointAccountResponse
{
    public string AccountNo { get; set; } = null!;
    public long CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = null!;
}