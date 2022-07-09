using Finbuckle.MultiTenant.Stores;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Persistence;
using FSH.WebApi.Infrastructure.Persistence.Configuration;
using FSH.WebApi.Shared.Multitenancy;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using SmartEnum.EFCore;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<FSHTenantInfo>
{
  private readonly DatabaseSettings _dbSettings;

  public TenantDbContext(DbContextOptions<TenantDbContext> options, IOptions<DatabaseSettings> dbSettings)
    : base(options)
  {
    _dbSettings = dbSettings.Value;
  }

  public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
  public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
  public DbSet<StandardSubscription> StandardSubscriptions => Set<StandardSubscription>();
  public DbSet<DemoSubscription> DemoSubscriptions => Set<DemoSubscription>();
  public DbSet<TrainSubscription> TrainSubscriptions => Set<TrainSubscription>();
  public DbSet<PaymentMethod> RootPaymentMethods => Set<PaymentMethod>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.ConfigureSmartEnum();

    modelBuilder
      .Entity<FSHTenantInfo>()
      .ToTable("Tenants", SchemaNames.MultiTenancy)
      .HasKey(a => a.Id);

    modelBuilder.Ignore<ActivePaymentOperation>();
    modelBuilder.Ignore<ArchivedPaymentOperation>();
    modelBuilder.Ignore<CashRegister>();
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);

    if (_dbSettings.LogSensitiveInfo)
    {
      optionsBuilder.EnableSensitiveDataLogging();
      optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    }
  }
}