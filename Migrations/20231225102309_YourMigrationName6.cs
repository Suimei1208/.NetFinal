using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetTechnology_Final.Migrations
{
    /// <inheritdoc />
    public partial class YourMigrationName6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UnitPrice",
                table: "OrderDetails",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "OrderDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
