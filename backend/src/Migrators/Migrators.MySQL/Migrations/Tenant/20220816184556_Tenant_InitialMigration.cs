using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.MySQL.Migrations.Tenant
{
    public partial class Tenant_InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MultiTenancy");

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RootPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CashDefault = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootPaymentMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(7,3)", precision: 7, scale: 3, nullable: false),
                    subscription_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Price = table.Column<decimal>(type: "decimal(7,3)", precision: 7, scale: 3, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantProdSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantProdSubscriptionId1 = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(7,3)", precision: 7, scale: 3, nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayments_RootPaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "RootPaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "MultiTenancy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Identifier = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VatNo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminPhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TechSupportUserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProdSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ConnectionString = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DemoSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DemoConnectionString = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrainSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TrainConnectionString = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Issuer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TenantSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(64)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tenant_subscription_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSubscription_Subscription_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantSubscription_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "MultiTenancy",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_TenantSubscriptionId",
                table: "SubscriptionHistories",
                column: "TenantSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_PaymentMethodId",
                table: "SubscriptionPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_TenantProdSubscriptionId",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_TenantProdSubscriptionId1",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DemoSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "DemoSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ProdSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "ProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TrainSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "TrainSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscription_SubscriptionId",
                table: "TenantSubscription",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscription_TenantId",
                table: "TenantSubscription",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistories_TenantSubscription_TenantSubscriptionId",
                table: "SubscriptionHistories",
                column: "TenantSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_TenantSubscription_TenantProdSubscript~1",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId1",
                principalTable: "TenantSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_TenantSubscription_TenantProdSubscripti~",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantSubscription_DemoSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "DemoSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantSubscription_ProdSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "ProdSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantSubscription_TrainSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "TrainSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantSubscription_DemoSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantSubscription_ProdSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantSubscription_TrainSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "SubscriptionHistories");

            migrationBuilder.DropTable(
                name: "SubscriptionPayments");

            migrationBuilder.DropTable(
                name: "RootPaymentMethods");

            migrationBuilder.DropTable(
                name: "TenantSubscription");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "MultiTenancy");
        }
    }
}
