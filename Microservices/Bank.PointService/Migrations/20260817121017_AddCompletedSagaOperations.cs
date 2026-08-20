using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.PointService.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedSagaOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "COMPLETED_SAGA_OPERATIONS",
                columns: table => new
                {
                    OPERATION_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    OPERATION_TYPE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    COMPLETED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMPLETED_SAGA_OPERATIONS", x => new { x.OPERATION_ID, x.OPERATION_TYPE });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COMPLETED_SAGA_OPERATIONS");
        }
    }
}
