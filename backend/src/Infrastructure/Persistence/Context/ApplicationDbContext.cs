using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartEnum.EFCore;

namespace FSH.WebApi.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
  public ApplicationDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser,
    ISerializerService serializer, ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings,
    IEventPublisher events)
    : base(currentTenant, options, currentUser, serializer, csBuilder, dbSettings, events)
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

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    IgnoreMultiTenantEntities(modelBuilder);

    modelBuilder.HasDefaultSchema(SchemaNames.Shared);

    modelBuilder.ConfigureSmartEnum();
  }

  private static void IgnoreMultiTenantEntities(ModelBuilder modelBuilder)
  {
    modelBuilder.Ignore<FSHTenantInfo>();
    modelBuilder.Ignore<Subscription>();
    modelBuilder.Ignore<SubscriptionPayment>();
    modelBuilder.Ignore<TenantSubscription>();
  }
}