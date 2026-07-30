using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.AccountService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAccountNoSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "ACCOUNT_NO_SEQ",
                startValue: 10000000L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "ACCOUNT_NO_SEQ",
                startValue: 1L);
        }
    }
}
