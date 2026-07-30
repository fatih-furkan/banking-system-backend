using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AccountService.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountNoSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ACCOUNT_NO_SEQ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "ACCOUNT_NO_SEQ");
        }
    }
}
