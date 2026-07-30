using Bank.CardService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.CardService.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasSequence<long>("CARD_NO_SEQ")
            .StartsAt(1)
            .IncrementsBy(1);
        
        modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("CARD");
                
                entity.HasKey(e => e.CardToken);

                entity.Property(e => e.CardToken)
                    .HasColumnName("CARD_TOKEN")
                    .HasMaxLength(40)
                    .ValueGeneratedNever();
                
                entity.Property(e => e.CardNo)
                    .HasColumnName("CARD_NO")
                    .HasMaxLength(16) ;
                
                entity.HasIndex(e => e.CardNo)
                    .IsUnique();
                
                entity.Property(e => e.CardAccountNo)
                    .HasColumnName("CARD_ACCOUNT_NO")
                    .HasMaxLength(8);
                
                entity.HasIndex(e => e.CardAccountNo)
                    .IsUnique();
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("CUSTOMER_ID")
                    .HasPrecision(18);
            }
        );
    }
}