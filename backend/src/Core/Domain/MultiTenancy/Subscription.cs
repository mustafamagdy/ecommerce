using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Domain.MultiTenancy;

public sealed class SubscriptionFeature : BaseEntity
{
  public SubscriptionFeatureType Feature { get; set; }
  public string Value { get; set; }

  public Guid PackageId { get; set; }
  public SubscriptionPackage Package { get; set; }
}

public sealed class SubscriptionPackage : BaseEntity
{
  public bool Default { get; set; }
  public int ValidForDays { get; set; }
  public decimal Price { get; set; }

  public List<SubscriptionFeature> Features { get; set; }
}