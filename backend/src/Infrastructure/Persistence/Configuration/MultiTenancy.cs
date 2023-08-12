using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.WebApi.Infrastructure.Persistence.Configuration;

public sealed class SubscriptionFeatureConfig : BaseEntityConfiguration<SubscriptionFeature, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<SubscriptionFeature> builder)
  {
    base.Configure(builder);

    builder.Property(a => a.Feature)
      .HasConversion(
        p => p.Name,
        p => SubscriptionFeatureType.FromValue(p));
  }
}

public sealed class SubscriptionPackageConfig : BaseEntityConfiguration<SubscriptionPackage, DefaultIdType>
{
  public override void Configure(EntityTypeBuilder<SubscriptionPackage> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);

    builder.HasMany(a => a.Features).WithOne(a => a.Package).HasForeignKey(a => a.PackageId);
  }
}

public sealed class TenantSubscriptionConfig : BaseEntityConfiguration<TenantSubscription, DefaultIdType>
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

public sealed class SubscriptionHistoryConfig : BaseAuditableEntityConfiguration<SubscriptionHistory>
{
  public override void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
  {
    base.Configure(builder);

    builder
      .Property(b => b.Price)
      .HasPrecision(7, 3);
  }
}

public sealed class SubscriptionPaymentConfig : BaseAuditableEntityConfiguration<SubscriptionPayment>
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