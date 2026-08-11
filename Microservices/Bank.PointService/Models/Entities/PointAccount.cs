namespace Bank.PointService.Models.Entities;

public class PointAccount
{
    public string AccountNo { get; set; } = null!;
    public long CustomerId { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = null!;
}