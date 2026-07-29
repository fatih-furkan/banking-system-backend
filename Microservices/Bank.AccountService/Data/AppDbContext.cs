using Bank.AccountService.Models.Entities;
using Bank.AccountService.Models.Entities.Account;
using Bank.AccountService.Models.Entities.Limit;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<ChargeLimit> ChargeLimits => Set<ChargeLimit>();
    public DbSet<CurrentChargeLimit> CurrentChargeLimits => Set<CurrentChargeLimit>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("ACCOUNT");
                
                entity.HasKey(e => e.AccountNo);
                
                entity.Property(e => e.AccountNo)
                    .HasColumnName("ACCOUNT_NO")
                    .HasMaxLength(8);
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID");
                entity.Property(e => e.BranchCode)
                    .HasColumnName("BRANCH_CODE")
                    .HasMaxLength(3);
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(2);
                entity.Property(e => e.Balance)
                    .HasColumnName("BALANCE");
            }
        );
        
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