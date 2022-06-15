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

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<FSHTenantInfo>
{
  private readonly DatabaseSettings _dbSettings;

  public TenantDbContext(DbContextOptions<TenantDbContext> options, IOptions<DatabaseSettings> dbSettings)
    : base(options)
  {
    _dbSettings = dbSettings.Value;
  }

  public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
  public DbSet<Subscription> Subscriptions => Set<Subscription>();
  public DbSet<PaymentMethod> RootPaymentMethods => Set<PaymentMethod>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .ToTable("Tenants", SchemaNames.MultiTenancy)
      .HasKey(a => a.Id);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .HasMany(a => a.Subscriptions)
      .WithOne(a => a.Tenant)
      .HasForeignKey(a => a.TenantId);

    modelBuilder
      .Entity<FSHTenantInfo>()
      .HasMany(a => a.Branches)
      .WithOne(a => a.Tenant)
      .HasForeignKey(a => a.TenantId);

    // modelBuilder.Entity<TenantSubscription>().ToTable("TenantSubscriptions", SchemaNames.MultiTenancy);
    // modelBuilder.Entity<Subscription>().ToTable("Subscriptions", SchemaNames.MultiTenancy);
    // modelBuilder.Entity<PaymentMethod>().ToTable("RootPaymentMethods", SchemaNames.MultiTenancy);
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