namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class GetCampaignsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public required List<Campaign> Data { get; set; }
}