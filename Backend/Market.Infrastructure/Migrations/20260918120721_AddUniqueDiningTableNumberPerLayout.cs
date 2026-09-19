using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDiningTableNumberPerLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiningTables_TableLayoutId",
                table: "DiningTables");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables",
                columns: new[] { "TableLayoutId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiningTables_TableLayoutId_Number",
                table: "DiningTables");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableLayoutId",
                table: "DiningTables",
                column: "TableLayoutId");
        }
    }
}
