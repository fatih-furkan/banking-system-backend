using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.PointService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "POINT_ACC_NO_SEQ");

            migrationBuilder.CreateTable(
                name: "POINT_ACCOUNT",
                columns: table => new
                {
                    ACCOUNT_NO = table.Column<string>(type: "NVARCHAR2(8)", maxLength: 8, nullable: false),
                    CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", maxLength: 18, nullable: false),
                    BALANCE = table.Column<decimal>(type: "DECIMAL(18, 2)", maxLength: 8, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(2000)", precision: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POINT_ACCOUNT", x => x.ACCOUNT_NO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POINT_ACCOUNT_CUSTOMER_ID",
                table: "POINT_ACCOUNT",
                column: "CUSTOMER_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POINT_ACCOUNT");

            migrationBuilder.DropSequence(
                name: "POINT_ACC_NO_SEQ");
        }
    }
}
