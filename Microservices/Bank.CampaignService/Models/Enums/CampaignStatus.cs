// ----------------------------------------------
// Represents the lifecycle status of a campaign.
// ----------------------------------------------

namespace Bank.CampaignService.Models.Enums
{
    public enum CampaignStatus
    {
        Draft = 0,  // Campaign is created but not yet published
        Active = 1,
        Passive = 2,


    }
}
