﻿// <auto-generated />
using System;
using FSH.WebApi.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Migrators.MySQL.Migrations.Tenant
{
    [DbContext(typeof(TenantDbContext))]
    [Migration("20220612170702_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.FSHTenantInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Address")
                        .HasColumnType("longtext");

                    b.Property<string>("AdminEmail")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("AdminName")
                        .HasColumnType("longtext");

                    b.Property<string>("AdminPhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("DatabaseName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Issuer")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<string>("TechSupportUserId")
                        .HasColumnType("longtext");

                    b.Property<string>("VatNo")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("Identifier")
                        .IsUnique();

                    b.ToTable("Tenants", "MultiTenancy");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.Subscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<int>("Days")
                        .HasColumnType("int");

                    b.Property<bool>("DefaultMonthly")
                        .HasColumnType("tinyint(1)");

                    b.Property<decimal>("MonthlyPrice")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<decimal?>("YearlyPrice")
                        .HasColumnType("decimal(65,30)");

                    b.HasKey("Id");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.SubscriptionPayment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(65,30)");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("DeletedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("LastModifiedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("PaymentMethodId")
                        .HasColumnType("char(36)");

                    b.Property<Guid?>("TenantSubscriptionId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("PaymentMethodId");

                    b.HasIndex("TenantSubscriptionId");

                    b.ToTable("SubscriptionPayment");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.TenantSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsDemo")
                        .HasColumnType("tinyint(1)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(65,30)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid>("SubscriptionId")
                        .HasColumnType("char(36)");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionId");

                    b.HasIndex("TenantId");

                    b.ToTable("TenantSubscriptions");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.Operation.PaymentMethod", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("CashDefault")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("RootPaymentMethods");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.Structure.Branch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<Guid?>("DeletedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("DeletedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<Guid>("LastModifiedBy")
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("LastModifiedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Branch");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.SubscriptionPayment", b =>
                {
                    b.HasOne("FSH.WebApi.Domain.Operation.PaymentMethod", "PaymentMethod")
                        .WithMany()
                        .HasForeignKey("PaymentMethodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FSH.WebApi.Domain.MultiTenancy.TenantSubscription", null)
                        .WithMany("Payments")
                        .HasForeignKey("TenantSubscriptionId");

                    b.Navigation("PaymentMethod");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.TenantSubscription", b =>
                {
                    b.HasOne("FSH.WebApi.Domain.MultiTenancy.Subscription", "Subscription")
                        .WithMany()
                        .HasForeignKey("SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FSH.WebApi.Domain.MultiTenancy.FSHTenantInfo", "Tenant")
                        .WithMany("Subscriptions")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subscription");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.Structure.Branch", b =>
                {
                    b.HasOne("FSH.WebApi.Domain.MultiTenancy.FSHTenantInfo", "Tenant")
                        .WithMany("Branches")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.FSHTenantInfo", b =>
                {
                    b.Navigation("Branches");

                    b.Navigation("Subscriptions");
                });

            modelBuilder.Entity("FSH.WebApi.Domain.MultiTenancy.TenantSubscription", b =>
                {
                    b.Navigation("Payments");
                });
#pragma warning restore 612, 618
        }
    }
}
