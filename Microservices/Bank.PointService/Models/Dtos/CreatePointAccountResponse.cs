namespace Bank.PointService.Models.Dtos;

public class CreatePointAccountResponse
{
    public required string AccountNo { get; set; } = null!;
    public required long CustomerId { get; set; }
    public required string Status { get; set; } = null!;
    public required decimal UsedPoint { get; set; }
    public required decimal EarnedPoint { get; set; }
    public required decimal ExpiredPoint { get; set; }
}