using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public abstract class SubscriptionConfig<T> : BaseEntityConfiguration<T, DefaultIdType>
  where T : Subscription
{
  public override void Configure(EntityTypeBuilder<T> builder)
  {
    base.Configure(builder);

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

public abstract class TenantSubscriptionConfig<T, TSubscription> : BaseEntityConfiguration<T, DefaultIdType>
  where T : TenantSubscription<TSubscription>
  where TSubscription : Subscription, new()
{
  public override void Configure(EntityTypeBuilder<T> builder)
  {
    base.Configure(builder);
    builder.HasMany(a => a.History).WithOne().HasForeignKey(a => a.TenantSubscriptionId);
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

public class SubscriptionHistoryConfig : BaseAuditableEntityConfiguration<SubscriptionHistory>
{
  public override void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);
  }
}

public class SubscriptionPaymentConfig : BaseAuditableEntityConfiguration<SubscriptionPayment>
{
  public override void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Amount)
      .HasPrecision(7, 3);
  }
}