using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Market.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiningTableAndTableLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "DiningTables",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#00ff00");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "DiningTables",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "rectangle");

            migrationBuilder.AddColumn<int>(
                name: "TableLayoutId",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "DiningTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TableLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "#ffffff"),
                    FloorImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableLayouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableLayoutId",
                table: "DiningTables",
                column: "TableLayoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables",
                column: "TableLayoutId",
                principalTable: "TableLayouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiningTables_TableLayouts_TableLayoutId",
                table: "DiningTables");

            migrationBuilder.DropTable(
                name: "TableLayouts");

            migrationBuilder.DropIndex(
                name: "IX_DiningTables_TableLayoutId",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "TableLayoutId",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "X",
                table: "DiningTables");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "DiningTables");
        }
    }
}
