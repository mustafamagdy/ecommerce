using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.MySQL.Migrations.Tenant
{
    public partial class Tenant_SubscriptionConfigs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "SubscriptionHistories",
                type: "varchar(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_TenantId",
                table: "SubscriptionHistories",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistories_Tenants_TenantId",
                table: "SubscriptionHistories",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_Subscription_SubscriptionId",
                table: "SubscriptionPayments",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionHistories_Tenants_TenantId",
                table: "SubscriptionHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_Subscription_SubscriptionId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_SubscriptionId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionHistories_TenantId",
                table: "SubscriptionHistories");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "SubscriptionHistories",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
