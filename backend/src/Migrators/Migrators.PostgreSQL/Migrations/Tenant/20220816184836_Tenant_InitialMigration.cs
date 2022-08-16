using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.PostgreSQL.Migrations.Tenant
{
    public partial class Tenant_InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MultiTenancy");

            migrationBuilder.CreateTable(
                name: "RootPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CashDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootPaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: false),
                    subscription_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantProdSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantProdSubscriptionId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(7,3)", precision: 7, scale: 3, nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "MultiTenancy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AdminEmail = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    VatNo = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    AdminName = table.Column<string>(type: "text", nullable: true),
                    AdminPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    TechSupportUserId = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ProdSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConnectionString = table.Column<string>(type: "text", nullable: true),
                    DemoSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    DemoConnectionString = table.Column<string>(type: "text", nullable: true),
                    TrainSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrainConnectionString = table.Column<string>(type: "text", nullable: true),
                    Issuer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false),
                    tenant_subscription_type = table.Column<string>(type: "text", nullable: false)
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
                });

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
                name: "FK_SubscriptionHistories_TenantSubscription_TenantSubscription~",
                table: "SubscriptionHistories",
                column: "TenantSubscriptionId",
                principalTable: "TenantSubscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_TenantSubscription_TenantProdSubscrip~1",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId1",
                principalTable: "TenantSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_TenantSubscription_TenantProdSubscript~",
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
