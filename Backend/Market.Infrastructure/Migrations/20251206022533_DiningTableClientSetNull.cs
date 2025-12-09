using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DiningTableClientSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables");

            migrationBuilder.DropForeignKey(
                name: "FK_TableReservations_DiningTables_DiningTableId",
                table: "TableReservations");

            migrationBuilder.AddForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables",
                column: "TableLayoutId",
                principalTable: "TableLayouts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservations_DiningTables_DiningTableId",
                table: "TableReservations",
                column: "DiningTableId",
                principalTable: "DiningTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables");

            migrationBuilder.DropForeignKey(
                name: "FK_TableReservations_DiningTables_DiningTableId",
                table: "TableReservations");

            migrationBuilder.AddForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables",
                column: "TableLayoutId",
                principalTable: "TableLayouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TableReservations_DiningTables_DiningTableId",
                table: "TableReservations",
                column: "DiningTableId",
                principalTable: "DiningTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
