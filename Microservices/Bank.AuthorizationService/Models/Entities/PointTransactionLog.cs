using Bank.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.TransactionService.Models.Entities
{
    [Table("POINT_TRANSACTION_LOG")]
    public class PointTransactionLog
    {
        [Key]
        [Column("LOG_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LogId { get; set; }

        [Column("CUSTOMER_ID")]
        public long CustomerId { get; set; }

        [Column("CAMPAIGN_ID")]
        public long? CampaignId { get; set; }

        [Column("TRANSACTION_ID")]
        [StringLength(50)]
        public string? TransactionId { get; set; }

        [Column("TRANSACTION_AMOUNT", TypeName = "NUMBER(18,2)")]
        public decimal TransactionAmount { get; set; }

        [Column("EARNED_POINT", TypeName = "NUMBER(18,2)")]
        public decimal EarnedPoint { get; set; }

        [Column("TRANSACTION_TYPE")]
        public PointTransactionType TransactionType { get; set; } = PointTransactionType.Earn;

        [Column("TRANSACTION_DATE", TypeName = "TIMESTAMP")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Column("DESCRIPTION")]
        [StringLength(250)]
        public string? Description { get; set; }
    }
}