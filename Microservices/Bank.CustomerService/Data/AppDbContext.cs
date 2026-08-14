using Bank.CustomerService.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace Bank.CustomerService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        modelBuilder.HasSequence<long>("CUSTOMER_ID_SEQ")
            .StartsAt(1)
            .IncrementsBy(1);
        
        modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("CUSTOMER");
                
                entity.HasKey(e => e.CustomerId);

                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18)
                    .ValueGeneratedNever();
                
                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasMaxLength(50);
                
                entity.Property(e => e.Surname)
                    .HasColumnName("SURNAME")
                    .HasMaxLength(50);
                
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(2);
                
                entity.Property(e => e.Tc)
                    .HasColumnName("TC")
                    .HasMaxLength(11);
            }
        );
    }
}