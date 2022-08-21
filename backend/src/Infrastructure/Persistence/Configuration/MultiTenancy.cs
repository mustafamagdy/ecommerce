using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public class SubscriptionConfig : BaseEntityConfiguration<Subscription, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<Subscription> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);

    builder
      .HasDiscriminator<string>("subscription_type")
      .HasValue<StandardSubscription>(SubscriptionType.Standard.Name)
      .HasValue<DemoSubscription>(SubscriptionType.Demo.Name)
      .HasValue<TrainSubscription>(SubscriptionType.Train.Name);
  }
}

public class TenantSubscriptionConfig : BaseEntityConfiguration<TenantSubscription, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<TenantSubscription> builder)
  {
    base.Configure(builder);
    builder.HasMany(a => a.History).WithOne(a => a.TenantSubscription).HasForeignKey(a => a.TenantSubscriptionId);

    builder
      .HasDiscriminator<string>("tenant_subscription_type")
      .HasValue<TenantProdSubscription>(SubscriptionType.Standard.Name)
      .HasValue<TenantDemoSubscription>(SubscriptionType.Demo.Name)
      .HasValue<TenantTrainSubscription>(SubscriptionType.Train.Name);
  }
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

    builder
      .HasOne(a => a.TenantProdSubscription)
      .WithMany(a => a.Payments)
      .HasForeignKey(a => a.TenantProdSubscriptionId);
  }
}