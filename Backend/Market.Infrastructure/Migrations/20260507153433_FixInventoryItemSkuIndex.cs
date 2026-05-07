using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryItemSkuIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_Sku",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_RestaurantId_Sku",
                table: "InventoryItems",
                columns: new[] { "RestaurantId", "Sku" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_RestaurantId_Sku",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Sku",
                table: "InventoryItems",
                column: "Sku",
                unique: true);
        }
    }
}
