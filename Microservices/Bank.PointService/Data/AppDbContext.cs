using Bank.PointService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.PointService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<PointAccount> PointAccounts => Set<PointAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasSequence<long>("POINT_ACC_NO_SEQ")
            .StartsAt(1)
            .IncrementsBy(1);
        
        modelBuilder.Entity<PointAccount>(entity =>
            {
                entity.ToTable("POINT_ACCOUNT");
                
                entity.HasKey(e => e.AccountNo);

                entity.Property(e => e.AccountNo)
                    .HasColumnName("ACCOUNT_NO")
                    .HasMaxLength(8);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18)
                    .IsRequired();
                
                entity.HasIndex(e => e.CustomerId)
                    .IsUnique();
                
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(2)
                    .IsRequired();
                
                entity.Property(e => e.EarnedPoint)
                    .HasColumnName("EARNED_POINT")
                    .HasPrecision(18,2)
                    .IsRequired();
                
                entity.Property(e => e.UsedPoint)
                    .HasColumnName("USED_POINT")
                    .HasPrecision(18,2)
                    .IsRequired();
                
                entity.Property(e => e.ExpiredPoint)
                    .HasColumnName("EXPIRED_POINT")
                    .HasPrecision(18,2)
                    .IsRequired();
            }
        );
    }
}