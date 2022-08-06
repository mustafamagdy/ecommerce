using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public abstract class SubscriptionConfig<T> : IEntityTypeConfiguration<T>
  where T : Subscription
{
  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);

    builder
      .Property(b => b.SubscriptionType)
      .HasConversion(
        p => p.Value,
        p => SubscriptionType.FromValue(p)
      );
  }
}

public class StandardSubscriptionConfig : SubscriptionConfig<StandardSubscription>
{
}

public class DemoSubscriptionConfig : SubscriptionConfig<DemoSubscription>
{
}

public class TrainSubscriptionConfig : SubscriptionConfig<TrainSubscription>
{
}

public abstract class TenantSubscriptionConfig<T, TSubscription> : IEntityTypeConfiguration<T>
  where T : TenantSubscription<TSubscription>
  where TSubscription : Subscription, new()

{
  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    builder.HasOne(a => a.Subscription).WithMany().HasForeignKey(a => a.SubscriptionId);
    builder.HasMany(a => a.History).WithOne().HasForeignKey(a => a.TenantSubscriptionId);
    builder.HasOne(a => a.Tenant).WithMany().HasForeignKey(a => a.TenantId);
  }
}

public class TenantProdSubscriptionConfig : TenantSubscriptionConfig<TenantProdSubscription, StandardSubscription>
{
  public override void Configure(EntityTypeBuilder<TenantProdSubscription> builder)
  {
    base.Configure(builder);
    builder.HasMany(a => a.Payments).WithOne().HasForeignKey(a => a.TenantProdSubscriptionId);
  }
}

public class TenantDemoSubscriptionConfig : TenantSubscriptionConfig<TenantDemoSubscription, DemoSubscription>
{
}

public class TenantTrainSubscriptionConfig : TenantSubscriptionConfig<TenantTrainSubscription, TrainSubscription>
{
}

public class SubscriptionHistoryConfig : IEntityTypeConfiguration<SubscriptionHistory>
{
  public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
  {
    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);
  }
}

public class SubscriptionPaymentConfig : IEntityTypeConfiguration<SubscriptionPayment>
{
  public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
  {
    builder
      .Property(b => b.Amount)
      .HasPrecision(7, 3);

    builder.HasOne(a => a.TenantProdSubscription).WithMany().HasForeignKey(a => a.TenantProdSubscriptionId);
  }
}