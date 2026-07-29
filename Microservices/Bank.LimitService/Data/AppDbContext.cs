using Bank.LimitService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.LimitService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<ChargeLimit> ChargeLimits => Set<ChargeLimit>();
    public DbSet<CurrentChargeLimit> CurrentChargeLimits => Set<CurrentChargeLimit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ChargeLimit>(entity =>
            {
                entity.ToTable("CHARGE_LIMITS");
                
                entity.HasKey(e => e.CustomerId);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.DailyLimit)
                    .HasColumnName("DAILY_LIMIT");
                entity.Property(e => e.MonthlyLimit)
                    .HasColumnName("MONTHLY_LIMIT");
                entity.Property(e => e.AnnualLimit)
                    .HasColumnName("ANNUAL_LIMIT");
            }
        );
        
        modelBuilder.Entity<CurrentChargeLimit>(entity =>
            {
                entity.ToTable("CURRENT_CHARGE_LIMITS");
                
                entity.HasKey(e => e.CustomerId);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.DailyLimit)
                    .HasColumnName("DAILY_LIMIT");
                entity.Property(e => e.MonthlyLimit)
                    .HasColumnName("MONTHLY_LIMIT");
                entity.Property(e => e.AnnualLimit)
                    .HasColumnName("ANNUAL_LIMIT");
                entity.Property(e => e.LastDailyReset)
                    .HasColumnName("LAST_DAILY_RESET");
                entity.Property(e => e.LastMonthlyReset)
                    .HasColumnName("LAST_MONTHLY_RESET");
                entity.Property(e => e.LastAnnualReset)
                    .HasColumnName("LAST_ANNUAL_RESET");
            }
        );
    }
}