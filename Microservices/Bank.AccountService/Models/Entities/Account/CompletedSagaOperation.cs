namespace Bank.AccountService.Models.Entities.Account;

public sealed class CompletedSagaOperation
{
    public Guid OperationId { get; set; }

    public string OperationType { get; set; } = null!;

    public DateTime CompletedAt { get; set; }
}