using Bank.PointService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.PointService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<PointAccount> PointAccounts => Set<PointAccount>();
    public DbSet<CompletedSagaOperation> CompletedSagaOperations { get; set; }
        = null!;
    
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