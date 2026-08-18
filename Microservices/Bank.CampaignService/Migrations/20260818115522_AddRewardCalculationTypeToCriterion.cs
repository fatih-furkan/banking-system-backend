using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CampaignService.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardCalculationTypeToCriterion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "REWARD_POINT",
                table: "CAMPAIGN_CRITERION",
                newName: "REWARD_CALCULATION_TYPE");

            migrationBuilder.AddColumn<decimal>(
                name: "REWARD_VALUE",
                table: "CAMPAIGN_CRITERION",
                type: "NUMBER(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "REWARD_VALUE",
                table: "CAMPAIGN_CRITERION");

            migrationBuilder.RenameColumn(
                name: "REWARD_CALCULATION_TYPE",
                table: "CAMPAIGN_CRITERION",
                newName: "REWARD_POINT");
        }
    }
}
