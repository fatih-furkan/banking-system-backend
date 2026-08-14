using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CampaignService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBudgetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TOTAL_BUDGET",
                table: "CAMPAIGN");

            migrationBuilder.DropColumn(
                name: "USED_BUDGET",
                table: "CAMPAIGN");

            migrationBuilder.AlterColumn<DateTime>(
                name: "START_DATE",
                table: "CAMPAIGN",
                type: "DATE",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "END_DATE",
                table: "CAMPAIGN",
                type: "DATE",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "START_DATE",
                table: "CAMPAIGN",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATE");

            migrationBuilder.AlterColumn<DateTime>(
                name: "END_DATE",
                table: "CAMPAIGN",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATE");

            migrationBuilder.AddColumn<decimal>(
                name: "TOTAL_BUDGET",
                table: "CAMPAIGN",
                type: "NUMBER(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "USED_BUDGET",
                table: "CAMPAIGN",
                type: "NUMBER(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
