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
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(64)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantDemoSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantProdSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantTrainSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionHistory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SubscriptionPayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantProdSubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayment_RootPaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "RootPaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TenantDemoSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(64)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDemoSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantDemoSubscription_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TenantProdSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(64)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantProdSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantProdSubscription_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
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
                    table.ForeignKey(
                        name: "FK_Tenants_TenantDemoSubscription_DemoSubscriptionId",
                        column: x => x.DemoSubscriptionId,
                        principalTable: "TenantDemoSubscription",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tenants_TenantProdSubscription_ProdSubscriptionId",
                        column: x => x.ProdSubscriptionId,
                        principalTable: "TenantProdSubscription",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TenantTrainSubscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(64)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTrainSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantTrainSubscription_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantTrainSubscription_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "MultiTenancy",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TenantId",
                table: "Branch",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistory_TenantDemoSubscriptionId",
                table: "SubscriptionHistory",
                column: "TenantDemoSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistory_TenantProdSubscriptionId",
                table: "SubscriptionHistory",
                column: "TenantProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistory_TenantTrainSubscriptionId",
                table: "SubscriptionHistory",
                column: "TenantTrainSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayment_PaymentMethodId",
                table: "SubscriptionPayment",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayment_TenantProdSubscriptionId",
                table: "SubscriptionPayment",
                column: "TenantProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantDemoSubscription_SubscriptionId",
                table: "TenantDemoSubscription",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantDemoSubscription_TenantId",
                table: "TenantDemoSubscription",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantProdSubscription_SubscriptionId",
                table: "TenantProdSubscription",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantProdSubscription_TenantId",
                table: "TenantProdSubscription",
                column: "TenantId");

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
                name: "IX_TenantTrainSubscription_SubscriptionId",
                table: "TenantTrainSubscription",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantTrainSubscription_TenantId",
                table: "TenantTrainSubscription",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_Tenants_TenantId",
                table: "Branch",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistory_TenantDemoSubscription_TenantDemoSubscri~",
                table: "SubscriptionHistory",
                column: "TenantDemoSubscriptionId",
                principalTable: "TenantDemoSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistory_TenantProdSubscription_TenantProdSubscri~",
                table: "SubscriptionHistory",
                column: "TenantProdSubscriptionId",
                principalTable: "TenantProdSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistory_TenantTrainSubscription_TenantTrainSubsc~",
                table: "SubscriptionHistory",
                column: "TenantTrainSubscriptionId",
                principalTable: "TenantTrainSubscription",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayment_TenantProdSubscription_TenantProdSubscri~",
                table: "SubscriptionPayment",
                column: "TenantProdSubscriptionId",
                principalTable: "TenantProdSubscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDemoSubscription_Tenants_TenantId",
                table: "TenantDemoSubscription",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProdSubscription_Tenants_TenantId",
                table: "TenantProdSubscription",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantTrainSubscription_TrainSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "TrainSubscriptionId",
                principalTable: "TenantTrainSubscription",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantDemoSubscription_Tenants_TenantId",
                table: "TenantDemoSubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantProdSubscription_Tenants_TenantId",
                table: "TenantProdSubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantTrainSubscription_Tenants_TenantId",
                table: "TenantTrainSubscription");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "SubscriptionHistory");

            migrationBuilder.DropTable(
                name: "SubscriptionPayment");

            migrationBuilder.DropTable(
                name: "RootPaymentMethods");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "MultiTenancy");

            migrationBuilder.DropTable(
                name: "TenantDemoSubscription");

            migrationBuilder.DropTable(
                name: "TenantProdSubscription");

            migrationBuilder.DropTable(
                name: "TenantTrainSubscription");

            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
