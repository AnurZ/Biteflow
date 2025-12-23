using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiningTablesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TableSize",
                table: "DiningTables");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "DiningTables");

            migrationBuilder.AddColumn<int>(
                name: "TableSize",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 100);
        }
    }
}
