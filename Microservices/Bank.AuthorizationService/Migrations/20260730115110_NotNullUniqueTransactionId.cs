using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AuthorizationService.Migrations
{
    /// <inheritdoc />
    public partial class NotNullUniqueTransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TRXN_ID",
                table: "AUTHORIZATION",
                type: "NUMBER(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(18,0)",
                oldPrecision: 18,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUTHORIZATION_TRXN_ID",
                table: "AUTHORIZATION",
                column: "TRXN_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AUTHORIZATION_TRXN_ID",
                table: "AUTHORIZATION");

            migrationBuilder.AlterColumn<long>(
                name: "TRXN_ID",
                table: "AUTHORIZATION",
                type: "NUMBER(18,0)",
                precision: 18,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(18,0)",
                oldPrecision: 18,
                oldScale: 0);
        }
    }
}
