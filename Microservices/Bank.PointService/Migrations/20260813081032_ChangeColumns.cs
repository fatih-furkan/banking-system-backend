using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.PointService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BALANCE",
                table: "POINT_ACCOUNT",
                newName: "USED_POINT");

            migrationBuilder.AddColumn<decimal>(
                name: "EARNED_POINT",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EXPIRED_POINT",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EARNED_POINT",
                table: "POINT_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "EXPIRED_POINT",
                table: "POINT_ACCOUNT");

            migrationBuilder.RenameColumn(
                name: "USED_POINT",
                table: "POINT_ACCOUNT",
                newName: "BALANCE");
        }
    }
}
