using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AccountService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNT",
                columns: table => new
                {
                    ACCOUNT_NO = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false),
                    BRANCH_CODE = table.Column<string>(type: "NVARCHAR2(3)", maxLength: 3, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    BALANCE = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNT", x => x.ACCOUNT_NO);
                });

            migrationBuilder.CreateTable(
                name: "CHARGE_LIMITS",
                columns: table => new
                {
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false),
                    DAILY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    MONTHLY_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    ANNUAL_LIMIT = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHARGE_LIMITS", x => x.CUSTOMER_ID);
                });

            migrationBuilder.CreateTable(
                name: "CURRENT_CHARGE_LIMITS",
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
                    table.PrimaryKey("PK_CURRENT_CHARGE_LIMITS", x => x.CUSTOMER_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCOUNT");

            migrationBuilder.DropTable(
                name: "CHARGE_LIMITS");

            migrationBuilder.DropTable(
                name: "CURRENT_CHARGE_LIMITS");
        }
    }
}
