using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CustomerService.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "CUSTOMER_ID_SEQ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "CUSTOMER_ID_SEQ");
        }
    }
}
