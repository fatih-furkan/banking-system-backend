using Bank.AuthorizationService.Models.Entities;
using Bank.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bank.AuthorizationService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Authorization> Authorizations => Set<Authorization>();
    public DbSet<SpendingLimit> SpendingLimits => Set<SpendingLimit>();
    public DbSet<CurrentSpendingLimit> CurrentSpendingLimits => Set<CurrentSpendingLimit>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.ToTable("AUTHORIZATION");

            entity.HasKey(e => e.Guid);

            entity.Property(e => e.Guid)
                .HasColumnName("GUID")
                .HasMaxLength(40)
                .IsRequired()
                .ValueGeneratedNever();

            entity.Property(e => e.TransactionStatus)
                .HasColumnName("TRXN_STATUS")
                .HasMaxLength(3);

            entity.Property(e => e.CustomerId)
                .HasColumnName("CUSTOMER_ID")
                .HasPrecision(18, 0)
                .IsRequired();

            entity.Property(e => e.CardToken)
                .HasColumnName("CARD_TOKEN")
                .HasMaxLength(30);

            entity.Property(e => e.TransactionDate)
                .HasColumnName("TRXN_DATE")
                .HasColumnType("DATE");

            entity.Property(e => e.Otc)
                .HasColumnName("OTC")
                .HasPrecision(4, 0)
                .IsRequired();

            entity.Property(e => e.Ots)
                .HasColumnName("OTS")
                .HasPrecision(4, 0)
                .IsRequired();

            entity.Property(e => e.TransactionDescription)
                .HasColumnName("TRXN_DESCRIPTION")
                .HasMaxLength(50);

            entity.Property(e => e.AccountNo)
                .HasColumnName("ACCOUNT_NO")
                .HasMaxLength(8);

            entity.Property(x => x.ChannelCode)
                .HasConversion(
                    value => value.ToDatabaseCode(),
                    value => ChannelCodeExtentions.FromDatabaseCode(value)
                )
                .HasColumnName("CHANNEL_CODE")
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(e => e.Balance)
                .HasColumnName("BALANCE")
                .HasPrecision(18, 2);

            entity.Property(e => e.TransactionAmount)
                .HasColumnName("TRXN_AMOUNT")
                .HasPrecision(18, 2);
            
            entity.Property(e => e.TransactionId)
                .HasColumnName("TRXN_ID")
                .HasPrecision(18, 0);
        });
        
        modelBuilder.Entity<SpendingLimit>(entity =>
            {
                entity.ToTable("SPENDING_LIMITS");
                
                entity.HasKey(e => e.CustomerId);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18)
                    .ValueGeneratedNever();
                
                
                entity.Property(e => e.DailyLimit)
                    .HasColumnName("DAILY_LIMIT")
                    .HasPrecision(18, 2);
                
                entity.Property(e => e.MonthlyLimit)
                    .HasColumnName("MONTHLY_LIMIT")
                    .HasPrecision(18, 2);
                entity.Property(e => e.AnnualLimit)
                    .HasColumnName("ANNUAL_LIMIT")
                    .HasPrecision(18, 2);
            }
        );
        
        modelBuilder.Entity<CurrentSpendingLimit>(entity =>
            {
                entity.ToTable("CURRENT_SPENDING_LIMITS");
                
                entity.HasKey(e => e.CustomerId);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18)
                    .ValueGeneratedNever();
                
                entity.Property(e => e.DailyLimit)
                    .HasColumnName("DAILY_LIMIT")
                    .HasPrecision(18, 2);
                
                entity.Property(e => e.MonthlyLimit)
                    .HasColumnName("MONTHLY_LIMIT")
                    .HasPrecision(18, 2);
                
                entity.Property(e => e.AnnualLimit)
                    .HasColumnName("ANNUAL_LIMIT")
                    .HasPrecision(18, 2);
                
                entity.Property(e => e.LastDailyReset)
                    .HasColumnName("LAST_DAILY_RESET")
                    .HasColumnType("DATE");
                
                entity.Property(e => e.LastMonthlyReset)
                    .HasColumnName("LAST_MONTHLY_RESET")
                    .HasColumnType("DATE");
                
                entity.Property(e => e.LastAnnualReset)
                    .HasColumnName("LAST_ANNUAL_RESET")
                    .HasColumnType("DATE");
            }
        );
    }
}