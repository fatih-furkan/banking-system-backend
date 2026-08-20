using Bank.Shared.Enums;

namespace Bank.AuthorizationService.Models.Dtos.ClientDtos;

public class CampaignCriterion
{
    
    public long CriterionId { get; set; }
    
    public long CampaignId { get; set; }
    
    public virtual Campaign? Campaign { get; set; }
    
    public RewardCalculationType RewardCalculationType { get; set; } = RewardCalculationType.Fixed;
    
    public decimal RewardValue { get; set; }

    
    public decimal? MinAmount { get; set; }

    public decimal? MaxAmount { get; set; }
    
    public string? Otc { get; set; }
    
    public DateTime? TransactionStartDate { get; set; }

    
    public DateTime? TransactionEndDate { get; set; }
    
    public bool IsFirstTransaction { get; set; } = false;

}