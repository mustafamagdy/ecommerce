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
                name: "DemoSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionType = table.Column<string>(type: "text", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoSubscriptions", x => x.Id);
                });

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
                name: "StandardSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionType = table.Column<string>(type: "text", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionType = table.Column<string>(type: "text", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantDemoSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantProdSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantTrainSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
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
                name: "TenantDemoSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantDemoSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantDemoSubscriptions_DemoSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "DemoSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantProdSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantProdSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantProdSubscriptions_StandardSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "StandardSubscriptions",
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
                    table.ForeignKey(
                        name: "FK_Tenants_TenantDemoSubscriptions_DemoSubscriptionId",
                        column: x => x.DemoSubscriptionId,
                        principalTable: "TenantDemoSubscriptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tenants_TenantProdSubscriptions_ProdSubscriptionId",
                        column: x => x.ProdSubscriptionId,
                        principalTable: "TenantProdSubscriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantTrainSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantTrainSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantTrainSubscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "MultiTenancy",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantTrainSubscriptions_TrainSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "TrainSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_TenantId",
                table: "Branch",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_TenantDemoSubscriptionId",
                table: "SubscriptionHistories",
                column: "TenantDemoSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_TenantProdSubscriptionId",
                table: "SubscriptionHistories",
                column: "TenantProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionHistories_TenantTrainSubscriptionId",
                table: "SubscriptionHistories",
                column: "TenantTrainSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_PaymentMethodId",
                table: "SubscriptionPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_TenantProdSubscriptionId",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantDemoSubscriptions_SubscriptionId",
                table: "TenantDemoSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantDemoSubscriptions_TenantId",
                table: "TenantDemoSubscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantProdSubscriptions_SubscriptionId",
                table: "TenantProdSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantProdSubscriptions_TenantId",
                table: "TenantProdSubscriptions",
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
                name: "IX_TenantTrainSubscriptions_SubscriptionId",
                table: "TenantTrainSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantTrainSubscriptions_TenantId",
                table: "TenantTrainSubscriptions",
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
                name: "FK_SubscriptionHistories_TenantDemoSubscriptions_TenantDemoSub~",
                table: "SubscriptionHistories",
                column: "TenantDemoSubscriptionId",
                principalTable: "TenantDemoSubscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistories_TenantProdSubscriptions_TenantProdSub~",
                table: "SubscriptionHistories",
                column: "TenantProdSubscriptionId",
                principalTable: "TenantProdSubscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionHistories_TenantTrainSubscriptions_TenantTrainS~",
                table: "SubscriptionHistories",
                column: "TenantTrainSubscriptionId",
                principalTable: "TenantTrainSubscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_TenantProdSubscriptions_TenantProdSubs~",
                table: "SubscriptionPayments",
                column: "TenantProdSubscriptionId",
                principalTable: "TenantProdSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDemoSubscriptions_Tenants_TenantId",
                table: "TenantDemoSubscriptions",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantProdSubscriptions_Tenants_TenantId",
                table: "TenantProdSubscriptions",
                column: "TenantId",
                principalSchema: "MultiTenancy",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantTrainSubscriptions_TrainSubscriptionId",
                schema: "MultiTenancy",
                table: "Tenants",
                column: "TrainSubscriptionId",
                principalTable: "TenantTrainSubscriptions",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantDemoSubscriptions_Tenants_TenantId",
                table: "TenantDemoSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantProdSubscriptions_Tenants_TenantId",
                table: "TenantProdSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantTrainSubscriptions_Tenants_TenantId",
                table: "TenantTrainSubscriptions");

            migrationBuilder.DropTable(
                name: "Branch");

            migrationBuilder.DropTable(
                name: "SubscriptionHistories");

            migrationBuilder.DropTable(
                name: "SubscriptionPayments");

            migrationBuilder.DropTable(
                name: "RootPaymentMethods");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "MultiTenancy");

            migrationBuilder.DropTable(
                name: "TenantDemoSubscriptions");

            migrationBuilder.DropTable(
                name: "TenantProdSubscriptions");

            migrationBuilder.DropTable(
                name: "TenantTrainSubscriptions");

            migrationBuilder.DropTable(
                name: "DemoSubscriptions");

            migrationBuilder.DropTable(
                name: "StandardSubscriptions");

            migrationBuilder.DropTable(
                name: "TrainSubscriptions");
        }
    }
}
