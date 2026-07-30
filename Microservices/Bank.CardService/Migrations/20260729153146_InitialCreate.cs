using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CardService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CARD",
                columns: table => new
                {
                    CARD_TOKEN = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false),
                    CARD_NO = table.Column<string>(type: "NVARCHAR2(16)", maxLength: 16, nullable: false),
                    CARD_ACCOUNT_NO = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARD", x => x.CARD_TOKEN);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CARD_CARD_ACCOUNT_NO",
                table: "CARD",
                column: "CARD_ACCOUNT_NO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CARD_CARD_NO",
                table: "CARD",
                column: "CARD_NO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CARD");
        }
    }
}
