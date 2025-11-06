using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    public partial class AddIdentityBridgeToEmployeeProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "EmployeeProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_ApplicationUserId",
                table: "EmployeeProfiles",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeProfiles_ApplicationUserId",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "EmployeeProfiles");
        }
    }
}
