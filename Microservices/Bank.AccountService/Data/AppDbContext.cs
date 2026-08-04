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
    
    public DbSet<CompletedSagaOperation> CompletedSagaOperations { get; set; }
        = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasSequence<long>("ACCOUNT_NO_SEQ")
            .StartsAt(10000000)
            .IncrementsBy(1);
        
        modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("ACCOUNT");
                
                entity.HasKey(e => e.AccountNo);
                
                entity.Property(e => e.AccountNo)
                    .HasColumnName("ACCOUNT_NO")
                    .HasMaxLength(8);
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18);
                entity.Property(e => e.BranchCode)
                    .HasColumnName("BRANCH_CODE")
                    .HasMaxLength(3);
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(2);
                entity.Property(e => e.Balance)
                    .HasColumnName("BALANCE")
                    .HasPrecision(18,2);
            }
        );
        
        modelBuilder.Entity<ChargeLimit>(entity =>
            {
                entity.ToTable("CHARGE_LIMITS");
                
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
        
        modelBuilder.Entity<CurrentChargeLimit>(entity =>
            {
                entity.ToTable("CURRENT_CHARGE_LIMITS");
                
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
        
        modelBuilder.Entity<CompletedSagaOperation>(entity =>
        {
            entity.ToTable("COMPLETED_SAGA_OPERATIONS");

            entity.HasKey(operation => new
            {
                operation.OperationId,
                operation.OperationType
            });

            entity.Property(operation => operation.OperationId)
                .HasColumnName("OPERATION_ID")
                .ValueGeneratedNever();

            entity.Property(operation => operation.OperationType)
                .HasColumnName("OPERATION_TYPE")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(operation => operation.CompletedAt)
                .HasColumnName("COMPLETED_AT")
                .IsRequired();
        });
    }
}