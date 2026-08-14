using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CampaignService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CAMPAIGN",
                columns: table => new
                {
                    Campaign_ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    START_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    END_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    REWARD_TYPE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CALCULATION_CRITERIA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TOTAL_BUDGET = table.Column<decimal>(type: "NUMBER(18,2)", nullable: false),
                    USED_BUDGET = table.Column<decimal>(type: "NUMBER(18,2)", nullable: false),
                    DAILY_LIMIT = table.Column<decimal>(type: "NUMBER(18,2)", nullable: true),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAMPAIGN", x => x.Campaign_ID);
                });

            migrationBuilder.CreateTable(
                name: "CAMPAIGN_CRITERION",
                columns: table => new
                {
                    CRITERION_ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CAMPAIGN_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    MIN_AMOUNTTypeNameNumber182 = table.Column<decimal>(name: "MIN_AMOUNT, TypeName = Number(18,2)", type: "DECIMAL(18, 2)", nullable: true),
                    MAX_AMOUNTTypeNameNumber182 = table.Column<decimal>(name: "MAX_AMOUNT, TypeName = Number(18,2)", type: "DECIMAL(18, 2)", nullable: true),
                    OTC = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    TRANSACTION_START_DATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    TRANSACTION_END_DATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    IS_FIRST_TRANSACTION = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAMPAIGN_CRITERION", x => x.CRITERION_ID);
                    table.ForeignKey(
                        name: "FK_CAMPAIGN_CRITERION_CAMPAIGN_CAMPAIGN_ID",
                        column: x => x.CAMPAIGN_ID,
                        principalTable: "CAMPAIGN",
                        principalColumn: "Campaign_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CAMPAIGN_CRITERION_CAMPAIGN_ID",
                table: "CAMPAIGN_CRITERION",
                column: "CAMPAIGN_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAMPAIGN_CRITERION");

            migrationBuilder.DropTable(
                name: "CAMPAIGN");
        }
    }
}
