using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bank.CampaignService.Models.Enums;
// ---------------------------------------------------
// Primary Campaign Entity (Mapped to CAMPAIGN table).
// Holds core campaign definitions, validity dates, rewards, and overall budget info.
// ---------------------------------------------------
namespace Bank.CampaignService.Models.Entities
{
    [Table("Campaign")]
    public class Campaign
    {

        [Key]                                                   // Primary key for campaign.
        [Column("Campaign_ID")]                                // CampaihnId = CAMPAIGN_ID column in the database.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-incremented by the database.
        public long CampaignId { get; set; }


        [Column("Name")]                                         // Name = NAME column in the database.)]

        [Required]                                                                      // ??? Required kullandık, getter ve setter metodlarında null olamaz gerekli midir?
        [StringLength (100)]
        public string Name { get; set; } = null!;


        [Column("Description")]
        [StringLength(500)]
        public string? Description { get; set; }


        [Column("START_DATE", TypeName = "DATE")]
        public DateTime StartDate { get; set; }


        [Column("END_DATE", TypeName = "DATE")]
        public DateTime EndDate { get; set; }


        [Column("REWARD_TYPE")]
        public RewardType RewardType { get; set; }


        [Column("CALCULATION_CRITERIA")]
        public CalculationCriteria CalculationCriteria { get; set; }


        /*
        
        [Column("TOTAL_BUDGET", TypeName = "NUMBER(18,2)")]
        /// </summary>
        public decimal TotalBudget { get; set; }


        [Column("USED_BUDGET", TypeName = "NUMBER(18,2)")]
        public decimal UsedBudget { get; set; }

        */
        

        [Column("DAILY_LIMIT", TypeName = "NUMBER(18,2)")]
        public decimal? DailyLimit { get; set; }


        [Column("STATUS")]
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        public virtual ICollection<CampaignCriterion> Criteria { get; set; } = new List<CampaignCriterion>();
    }

}

