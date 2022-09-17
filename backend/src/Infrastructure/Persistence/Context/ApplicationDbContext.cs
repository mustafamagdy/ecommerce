using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Configuration;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartEnum.EFCore;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public sealed class ApplicationDbContext : BaseDbContext
{
  public ApplicationDbContext(ITenantInfo currentTenant, ISubscriptionTypeResolver subscriptionTypeResolver,
    DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser, ISerializerService serializer,
    ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings, ITenantConnectionStringResolver tenantConnectionStringResolver)
    : base(currentTenant, options, currentUser, serializer, csBuilder, dbSettings, subscriptionTypeResolver, tenantConnectionStringResolver)
  {
  }

  public DbSet<Branch> Branches => Set<Branch>();
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Brand> Brands => Set<Brand>();
  public DbSet<ServiceCatalog> ServiceCatalogs => Set<ServiceCatalog>();
  public DbSet<Service> Services => Set<Service>();
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();
  public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
  public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
  public DbSet<CashRegister> CashRegisters => Set<CashRegister>();

  public DbSet<PaymentOperation> PaymentOperations => Set<PaymentOperation>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    if (Database.IsNpgsql())
    {
      AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<PaymentMethod>().IsMultiTenant();

    IgnoreMultiTenantEntities(modelBuilder);

    modelBuilder.HasDefaultSchema(SchemaNames.Shared);

    modelBuilder.ConfigureSmartEnum();
  }

  private static void IgnoreMultiTenantEntities(ModelBuilder modelBuilder)
  {
    modelBuilder.Ignore<FSHTenantInfo>();
    modelBuilder.Ignore<TenantSubscription>();
    modelBuilder.Ignore<TenantProdSubscription>();
    modelBuilder.Ignore<TenantDemoSubscription>();
    modelBuilder.Ignore<TenantTrainSubscription>();
    modelBuilder.Ignore<SubscriptionFeature>();
    modelBuilder.Ignore<SubscriptionPackage>();
    modelBuilder.Ignore<SubscriptionPayment>();
    modelBuilder.Ignore<SubscriptionHistory>();
  }
}