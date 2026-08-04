namespace Bank.AccountService.Models.Dtos.Limit;

public class CompensateUseChargeLimitRequest
{
    public Guid OperationId { get; set; }

    public long CustomerId { get; set; }

    public decimal Amount { get; set; }
}