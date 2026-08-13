namespace Bank.PointService.Models.Entities;

public class PointAccount
{
    public string AccountNo { get; set; } = null!;
    public long CustomerId { get; set; }
    public string Status { get; set; } = null!;
    public decimal EarnedPoint { get; set; }
    public decimal UsedPoint { get; set; }
    public decimal ExpiredPoint { get; set; }
}