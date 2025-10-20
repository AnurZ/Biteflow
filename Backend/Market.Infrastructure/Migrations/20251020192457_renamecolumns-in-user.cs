using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class renamecolumnsinuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Users",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "RefreshTokens",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RefreshTokens",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Products",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Products",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "ProductCategories",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ProductCategories",
                newName: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                table: "Users",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                table: "RefreshTokens",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "RefreshTokens",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                table: "Products",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Products",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                table: "ProductCategories",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "ProductCategories",
                newName: "CreatedAt");
        }
    }
}
