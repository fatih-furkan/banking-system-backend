using Bank.CampaignService.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bank.CampaignService.Models.Entities
{

    [Table("CAMPAIGN_CRITERION")]
    public class CampaignCriterion
    {

        [Key]                                                      // Primary key
        [Column("CRITERION_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CriterionId { get; set; }                     // Unique id of Criterion


        [Column("CAMPAIGN_ID")]                                  // Foreign key to Campaign table
        public long CampaignId { get; set; }

        [JsonIgnore]
        [NotMapped]
        public virtual Campaign? Campaign { get; set; }

        
        [Column("REWARD_CALCULATION_TYPE")]
        public RewardCalculationType RewardCalculationType { get; set; } = RewardCalculationType.Fixed;

        [Column("REWARD_VALUE", TypeName = "NUMBER(18,2)")]
        public decimal RewardValue { get; set; }


        [Column("MIN_AMOUNT", TypeName = "NUMBER(18,2)")]
        public decimal? MinAmount { get; set; }

        [Column("MAX_AMOUNT", TypeName = "NUMBER(18,2)")]
        public decimal? MaxAmount { get; set; }


        [Column("OTC")]
        [StringLength(10)]
        public string? Otc { get; set; }


        [Column("TRANSACTION_START_DATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? TransactionStartDate { get; set; }


        [Column("TRANSACTION_END_DATE", TypeName = "DATE")]
        [DataType(DataType.Date)]
        public DateTime? TransactionEndDate { get; set; }


        [Column("IS_FIRST_TRANSACTION", TypeName = "NUMBER(1)")]
        public bool IsFirstTransaction { get; set; } = false;

    }
}
