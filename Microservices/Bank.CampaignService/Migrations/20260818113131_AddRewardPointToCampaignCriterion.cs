using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CampaignService.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardPointToCampaignCriterion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "REWARD_POINT",
                table: "CAMPAIGN_CRITERION",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "REWARD_POINT",
                table: "CAMPAIGN_CRITERION");
        }
    }
}
