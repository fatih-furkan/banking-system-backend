namespace Bank.CardService.Models.Dtos.ClientDtos.SagaDtos;

public class CreateCardSagaRequest
{
    public long CustomerId { get; set; }
    public string BranchCode { get; set; } = null!;
}