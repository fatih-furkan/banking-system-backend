using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.CustomerService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CUSTOMER",
                columns: table => new
                {
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(18)", precision: 18, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    SURNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    TC = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER", x => x.CUSTOMER_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CUSTOMER");
        }
    }
}
