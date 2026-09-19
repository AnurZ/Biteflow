using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeDiningTableNumberUniqueForActiveTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables",
                columns: new[] { "TableLayoutId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables",
                columns: new[] { "TableLayoutId", "Number" },
                unique: true);
        }
    }
}
