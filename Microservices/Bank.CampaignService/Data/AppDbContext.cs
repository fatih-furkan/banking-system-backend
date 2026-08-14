using Bank.CampaignService.Models.Entities;
using Microsoft.EntityFrameworkCore;

// ----------------------------------------------------------------
// Acts as a bridge between C# entities and Oracle database tables.
// ----------------------------------------------------------------

namespace Bank.CampaignService.Data
{
    public class AppDbContext : DbContext
    {
        // DbContext constructor accepting options (connection string, provider settings, etc.)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet properties representing database tables.
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<CampaignCriterion> CampaignCriteria => Set<CampaignCriterion>();




        // Configures database table mappings, keys, constraints, and relationships.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // 1. CAMPAIGN TABLE MAPPING
            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("CAMPAIGN");
                entity.HasKey(e => e.CampaignId);
            });


            // 2. CAMPAIGN_CRITERION TABLE MAPPING
            modelBuilder.Entity<CampaignCriterion>(entity =>
            {
                entity.ToTable("CAMPAIGN_CRITERION");
                entity.HasKey(e => e.CriterionId);

                // Prevents EF Core from generating unwanted shadow properties (e.g., CampaignId1
              // ensuring clean mapping with Oracle columns.
              //////////  entity.Ignore(c => c.Campaign);
            });

        }

    }
}