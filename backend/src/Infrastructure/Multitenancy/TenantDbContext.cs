using Finbuckle.MultiTenant.Stores;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Persistence.Configuration;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<FSHTenantInfo>
{
  public TenantDbContext(DbContextOptions<TenantDbContext> options)
    : base(options)
  {
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

    modelBuilder.Entity<TenantSubscription>().ToTable("TenantSubscriptions", SchemaNames.MultiTenancy);
    modelBuilder.Entity<Subscription>().ToTable("Subscriptions", SchemaNames.MultiTenancy);
    modelBuilder.Entity<PaymentMethod>().ToTable("RootPaymentMethods", SchemaNames.MultiTenancy);
  }
}