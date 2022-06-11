using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class TenantSubscriptionConfig : IEntityTypeConfiguration<TenantSubscription>
{
  public void Configure(EntityTypeBuilder<TenantSubscription> builder)
  {
    builder
      .HasOne<FSHTenantInfo>()
      .WithMany(a => a.Subscriptions)
      .HasForeignKey(a => a.TenantId);
  }
}

public class SubscriptionConfig : IEntityTypeConfiguration<Subscription>
{
  public void Configure(EntityTypeBuilder<Subscription> builder)
  {
    builder
      .Property(a => a.MonthlyPrice)
      .HasPrecision(7, 3);

    builder
      .Property(a => a.YearlyPrice)
      .HasPrecision(7, 3);
  }
}