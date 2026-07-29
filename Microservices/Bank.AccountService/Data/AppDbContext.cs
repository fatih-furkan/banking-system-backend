using Bank.AccountService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.AccountService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Account> Accounts => Set<Account>();

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
    }
}