using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTablesReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableReservations_AspNetUsers_ApplicationUserId",
                table: "TableReservations");

            migrationBuilder.DropIndex(
                name: "IX_TableReservations_DiningTableId",
                table: "TableReservations");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TableReservations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TableReservations_DiningTableId_ReservationStart",
                table: "TableReservations",
                columns: new[] { "DiningTableId", "ReservationStart" });

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservations_AspNetUsers_ApplicationUserId",
                table: "TableReservations",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableReservations_AspNetUsers_ApplicationUserId",
                table: "TableReservations");

            migrationBuilder.DropIndex(
                name: "IX_TableReservations_DiningTableId_ReservationStart",
                table: "TableReservations");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TableReservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableReservations_DiningTableId",
                table: "TableReservations",
                column: "DiningTableId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservations_AspNetUsers_ApplicationUserId",
                table: "TableReservations",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
