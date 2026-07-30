using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CardService.Migrations
{
    /// <inheritdoc />
    public partial class AddCardNoSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "CARD_NO_SEQ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "CARD_NO_SEQ");
        }
    }
}
