using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Accounting; // Add this for Accounting entities
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
  public ApplicationDbContext(ITenantInfo currentTenant, ISubscriptionInfo subscriptionInfo, DbContextOptions options, ICurrentUser currentUser,
    ISerializerService serializer, ITenantConnectionStringBuilder csBuilder, IOptions<DatabaseSettings> dbSettings,
    IEventPublisher events, TenantDbContext tenantDb)
    : base(currentTenant, options, currentUser, serializer, csBuilder, dbSettings, events, subscriptionInfo, tenantDb)
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

  // Accounting DbSets
  public DbSet<Account> Accounts => Set<Account>();
  public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
  public DbSet<Transaction> Transactions => Set<Transaction>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    IgnoreMultiTenantEntities(modelBuilder);

    modelBuilder.HasDefaultSchema(SchemaNames.Shared); // This is for shared tables. Accounting might be tenant-specific.

    // Apply configurations from the assembly, including Accounting configurations
    // modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    // It's often better to be explicit if you have schemas or specific loading needs.
    // The existing project structure might already call ApplyConfigurationsFromAssembly in BaseDbContext
    // or expect configurations to be added there.
    // For now, let's assume BaseDbContext handles ApplyConfigurationsFromAssembly.
    // If it doesn't, this is where you'd add it:
    // modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountConfiguration).Assembly);

    modelBuilder.ConfigureSmartEnum();
  }

  private static void IgnoreMultiTenantEntities(ModelBuilder modelBuilder)
  {
    modelBuilder.Ignore<FSHTenantInfo>();
    modelBuilder.Ignore<Subscription>();
    modelBuilder.Ignore<StandardSubscription>();
    modelBuilder.Ignore<DemoSubscription>();
    modelBuilder.Ignore<TrainSubscription>();
    modelBuilder.Ignore<SubscriptionPayment>();
    modelBuilder.Ignore<SubscriptionHistory>();
  }
}