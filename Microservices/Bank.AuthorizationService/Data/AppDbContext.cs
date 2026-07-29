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
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.ToTable("AUTHORIZATION"); // Gerçek tablo adını yaz

            entity.HasKey(e => e.Guid);

            entity.Property(e => e.Guid)
                .HasColumnName("GUID")
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(e => e.TransactionStatus)
                .HasColumnName("TRXN_STATUS")
                .HasMaxLength(3);

            entity.Property(e => e.CustomerId)
                .HasColumnName("CUSTOMER_ID")
                .HasPrecision(20, 0)
                .IsRequired();

            entity.Property(e => e.CardToken)
                .HasColumnName("CARD_TOKEN")
                .HasMaxLength(100);

            entity.Property(e => e.TransactionDate)
                .HasColumnName("TRXN_DATE");

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
                .HasColumnName("TXN_AMOUNT")
                .HasPrecision(18, 2);
        });
    }
}