using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.PointService.Migrations
{
    /// <inheritdoc />
    public partial class PrecisionFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "STATUS",
                table: "POINT_ACCOUNT",
                type: "NVARCHAR2(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldPrecision: 18);

            migrationBuilder.AlterColumn<long>(
                name: "CUSTOMER_ID",
                table: "POINT_ACCOUNT",
                type: "NUMBER(18)",
                precision: 18,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldMaxLength: 18);

            migrationBuilder.AlterColumn<decimal>(
                name: "BALANCE",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(8)",
                precision: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)",
                oldMaxLength: 8);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "STATUS",
                table: "POINT_ACCOUNT",
                type: "NVARCHAR2(2000)",
                precision: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<long>(
                name: "CUSTOMER_ID",
                table: "POINT_ACCOUNT",
                type: "NUMBER(19)",
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(18)",
                oldPrecision: 18);

            migrationBuilder.AlterColumn<decimal>(
                name: "BALANCE",
                table: "POINT_ACCOUNT",
                type: "DECIMAL(18,2)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(8)",
                oldPrecision: 8);
        }
    }
}
