using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AuthorizationService.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundedAmountAndOriginalTrxnId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ORIGINAL_TRXN_ID",
                table: "AUTHORIZATION",
                type: "NUMBER(18)",
                precision: 18,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "REFUNDED_AMOUNT",
                table: "AUTHORIZATION",
                type: "DECIMAL(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ORIGINAL_TRXN_ID",
                table: "AUTHORIZATION");

            migrationBuilder.DropColumn(
                name: "REFUNDED_AMOUNT",
                table: "AUTHORIZATION");
        }
    }
}
