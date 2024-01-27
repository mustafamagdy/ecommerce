using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.PostgreSQL.Migrations.Application
{
    /// <inheritdoc />
    public partial class App_AddCategoryToCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "Shared",
                table: "ServiceCatalogs");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCatalogs_CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCatalogs_Categories_CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs",
                column: "CategoryId",
                principalSchema: "Shared",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCatalogs_Categories_CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "Shared");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCatalogs_CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "Shared",
                table: "ServiceCatalogs");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                schema: "Shared",
                table: "ServiceCatalogs",
                type: "text",
                nullable: false,
                defaultValue: "normal");
        }
    }
}
