using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AuthorizationService.Migrations
{
    /// <inheritdoc />
    public partial class AddPointTransactionLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POINT_TRANSACTION_LOG",
                columns: table => new
                {
                    LOG_ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CAMPAIGN_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    TRANSACTION_ID = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    TRANSACTION_AMOUNT = table.Column<decimal>(type: "NUMBER(18,2)", nullable: false),
                    EARNED_POINT = table.Column<decimal>(type: "NUMBER(18,2)", nullable: false),
                    TRANSACTION_TYPE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TRANSACTION_DATE = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POINT_TRANSACTION_LOG", x => x.LOG_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POINT_TRANSACTION_LOG");
        }
    }
}
