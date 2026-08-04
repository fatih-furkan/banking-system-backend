namespace Bank.AuthorizationService.Models.Entities;

public class CompletedSagaOperation
{
    public Guid OperationId { get; set; }

    public string OperationType { get; set; } = null!;

    public DateTime CompletedAt { get; set; }
}