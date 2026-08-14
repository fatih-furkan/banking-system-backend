using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CampaignService.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MIN_AMOUNT, TypeName = Number(18,2)",
                table: "CAMPAIGN_CRITERION",
                newName: "MIN_AMOUNT");

            migrationBuilder.RenameColumn(
                name: "MAX_AMOUNT, TypeName = Number(18,2)",
                table: "CAMPAIGN_CRITERION",
                newName: "MAX_AMOUNT");

            migrationBuilder.AlterColumn<decimal>(
                name: "MIN_AMOUNT",
                table: "CAMPAIGN_CRITERION",
                type: "NUMBER(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MAX_AMOUNT",
                table: "CAMPAIGN_CRITERION",
                type: "NUMBER(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MIN_AMOUNT",
                table: "CAMPAIGN_CRITERION",
                newName: "MIN_AMOUNT, TypeName = Number(18,2)");

            migrationBuilder.RenameColumn(
                name: "MAX_AMOUNT",
                table: "CAMPAIGN_CRITERION",
                newName: "MAX_AMOUNT, TypeName = Number(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MIN_AMOUNT, TypeName = Number(18,2)",
                table: "CAMPAIGN_CRITERION",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MAX_AMOUNT, TypeName = Number(18,2)",
                table: "CAMPAIGN_CRITERION",
                type: "DECIMAL(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "NUMBER(18,2)",
                oldNullable: true);
        }
    }
}
