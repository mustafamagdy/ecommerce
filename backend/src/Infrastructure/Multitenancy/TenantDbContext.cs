using System.Diagnostics;
using Finbuckle.MultiTenant.Stores;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Persistence;
using FSH.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
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

    if (this.Database.IsNpgsql())
    {
      AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
  }

  public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
  public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();

  // private DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

  public DbSet<TenantProdSubscription> TenantProdSubscriptions => Set<TenantProdSubscription>();
  public DbSet<TenantDemoSubscription> TenantDemoSubscriptions => Set<TenantDemoSubscription>();

  public DbSet<TenantTrainSubscription> TenantTrainSubscriptions => Set<TenantTrainSubscription>();

  // private DbSet<Subscription> Subscriptions => Set<Subscription>();
  public DbSet<StandardSubscription> StandardSubscriptions => Set<StandardSubscription>();
  public DbSet<DemoSubscription> DemoSubscriptions => Set<DemoSubscription>();
  public DbSet<TrainSubscription> TrainSubscriptions => Set<TrainSubscription>();

  public DbSet<PaymentMethod> RootPaymentMethods => Set<PaymentMethod>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    modelBuilder.ApplyConfiguration(new SubscriptionConfig());
    modelBuilder.ApplyConfiguration(new TenantSubscriptionConfig());
    modelBuilder.ApplyConfiguration(new SubscriptionHistoryConfig());
    modelBuilder.ApplyConfiguration(new SubscriptionPaymentConfig());

    modelBuilder.ConfigureSmartEnum();

    modelBuilder
      .Entity<FSHTenantInfo>()
      .ToTable("Tenants", SchemaNames.MultiTenancy)
      .HasKey(a => a.Id);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .HasOne(a => a.ProdSubscription).WithMany().HasForeignKey(a => a.ProdSubscriptionId);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .HasOne(a => a.DemoSubscription).WithMany().HasForeignKey(a => a.DemoSubscriptionId);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .HasOne(a => a.TrainSubscription).WithMany().HasForeignKey(a => a.TrainSubscriptionId);

    IgnoredEntities(modelBuilder);
  }

  private static void IgnoredEntities(ModelBuilder modelBuilder)
  {
    modelBuilder.Ignore<Branch>();
    modelBuilder.Ignore<ActivePaymentOperation>();
    modelBuilder.Ignore<ArchivedPaymentOperation>();
    modelBuilder.Ignore<CashRegister>();
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);

    if (_dbSettings.LogSensitiveInfo)
    {
      optionsBuilder.EnableDetailedErrors();
      optionsBuilder.EnableSensitiveDataLogging();
      optionsBuilder.LogTo(m => Debug.WriteLine(m), LogLevel.Information);
      optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
      optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
    }
  }
}