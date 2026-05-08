using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreTableLayoutRestaurantOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "TableLayouts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE layouts
                SET RestaurantId = restaurants.Id
                FROM TableLayouts AS layouts
                CROSS APPLY (
                    SELECT TOP (1) Id
                    FROM Restaurants
                    WHERE TenantId = layouts.TenantId
                    ORDER BY CreatedAtUtc, Id
                ) AS restaurants
                WHERE layouts.RestaurantId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TableLayouts_RestaurantId",
                table: "TableLayouts",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TableLayouts_RestaurantId",
                table: "TableLayouts");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "TableLayouts");
        }
    }
}
