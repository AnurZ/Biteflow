using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations;

/// <inheritdoc />
public partial class productIsEnabled : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsEnabled",
            table: "ProductCategories",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsEnabled",
            table: "ProductCategories");
    }
}
