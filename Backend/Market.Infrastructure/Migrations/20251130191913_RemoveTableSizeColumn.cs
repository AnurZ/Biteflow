using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTableSizeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "DiningTables");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "DiningTables",
                newName: "TableSize");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TableSize",
                table: "DiningTables",
                newName: "Width");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 100);
        }
    }
}
