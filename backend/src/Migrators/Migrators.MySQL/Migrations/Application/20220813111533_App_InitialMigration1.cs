using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.MySQL.Migrations.Application
{
    public partial class App_InitialMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColumnDefs",
                schema: "Shared",
                table: "DocumentSection",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeaderStyle",
                schema: "Shared",
                table: "DocumentSection",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeaderTitle",
                schema: "Shared",
                table: "DocumentSection",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnDefs",
                schema: "Shared",
                table: "DocumentSection");

            migrationBuilder.DropColumn(
                name: "HeaderStyle",
                schema: "Shared",
                table: "DocumentSection");

            migrationBuilder.DropColumn(
                name: "HeaderTitle",
                schema: "Shared",
                table: "DocumentSection");
        }
    }
}
