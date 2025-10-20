using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations;

/// <inheritdoc />
public partial class user_is_admin : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Role",
            table: "Users");

        migrationBuilder.AddColumn<bool>(
            name: "IsAdmin",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsEmployee",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsManager",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "RevokedAtUtc",
            table: "RefreshTokens",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsAdmin",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IsEmployee",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IsManager",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "RevokedAtUtc",
            table: "RefreshTokens");

        migrationBuilder.AddColumn<string>(
            name: "Role",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
