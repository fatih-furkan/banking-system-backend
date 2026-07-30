using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AuthorizationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AUTHORIZATION",
                columns: table => new
                {
                    GUID = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false),
                    TRXN_STATUS = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: true),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18,0)", precision: 18, scale: 0, nullable: false),
                    CARD_TOKEN = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    TRXN_DATE = table.Column<DateTime>(type: "DATE", nullable: true),
                    OTC = table.Column<int>(type: "NUMBER(4,0)", precision: 4, scale: 0, nullable: false),
                    OTS = table.Column<int>(type: "NUMBER(4,0)", precision: 4, scale: 0, nullable: false),
                    TRXN_DESCRIPTION = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ACCOUNT_NO = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: true),
                    CHANNEL_CODE = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false),
                    BALANCE = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: true),
                    TRXN_AMOUNT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: true),
                    TRXN_ID = table.Column<long>(type: "NUMBER(18,0)", precision: 18, scale: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTHORIZATION", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "CURRENT_SPENDING_LIMITS",
                columns: table => new
                {
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false),
                    DAILY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    MONTHLY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    ANNUAL_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    LAST_DAILY_RESET = table.Column<DateTime>(type: "DATE", nullable: false),
                    LAST_MONTHLY_RESET = table.Column<DateTime>(type: "DATE", nullable: false),
                    LAST_ANNUAL_RESET = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURRENT_SPENDING_LIMITS", x => x.CUSTOMER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SPENDING_LIMITS",
                columns: table => new
                {
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false),
                    DAILY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    MONTHLY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    ANNUAL_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPENDING_LIMITS", x => x.CUSTOMER_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AUTHORIZATION");

            migrationBuilder.DropTable(
                name: "CURRENT_SPENDING_LIMITS");

            migrationBuilder.DropTable(
                name: "SPENDING_LIMITS");
        }
    }
}
