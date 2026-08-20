using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class Campaign
{
    public long CampaignId { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public RewardType RewardType { get; set; }
    
    public CalculationCriteria CalculationCriteria { get; set; }
    
    public decimal? DailyLimit { get; set; }
    
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    public virtual ICollection<CampaignCriterion> Criteria { get; set; } = new List<CampaignCriterion>();
}