using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.PointService.Migrations
{
    /// <inheritdoc />
    public partial class PrecisionFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BALANCE",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(8)",
                oldPrecision: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BALANCE",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(8)",
                precision: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
