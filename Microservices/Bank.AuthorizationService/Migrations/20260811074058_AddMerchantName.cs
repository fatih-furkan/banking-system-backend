using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AuthorizationService.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MERCHANT_NAME",
                table: "AUTHORIZATION",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MERCHANT_NAME",
                table: "AUTHORIZATION");
        }
    }
}
