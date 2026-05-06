using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableLayoutTenantOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TableLayouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TableLayouts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAtUtc",
                table: "TableLayouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "TableLayouts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                DECLARE @DefaultTenantId uniqueidentifier = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
                DECLARE @DefaultRestaurantId uniqueidentifier = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';

                UPDATE TableLayouts
                SET RestaurantId = @DefaultRestaurantId
                WHERE RestaurantId IS NULL
                   OR RestaurantId = '00000000-0000-0000-0000-000000000000';

                UPDATE layouts
                SET TenantId = COALESCE(restaurants.TenantId, @DefaultTenantId)
                FROM TableLayouts AS layouts
                LEFT JOIN Restaurants AS restaurants ON restaurants.Id = layouts.RestaurantId;

                UPDATE TableLayouts
                SET CreatedAtUtc = SYSUTCDATETIME()
                WHERE CreatedAtUtc IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "RestaurantId",
                table: "TableLayouts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "TableLayouts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "TableLayouts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableLayouts_RestaurantId",
                table: "TableLayouts",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_TableLayouts_TenantId",
                table: "TableLayouts",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TableLayouts_RestaurantId",
                table: "TableLayouts");

            migrationBuilder.DropIndex(
                name: "IX_TableLayouts_TenantId",
                table: "TableLayouts");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "TableLayouts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TableLayouts");

            migrationBuilder.DropColumn(
                name: "ModifiedAtUtc",
                table: "TableLayouts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TableLayouts");

            migrationBuilder.AlterColumn<Guid>(
                name: "RestaurantId",
                table: "TableLayouts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
