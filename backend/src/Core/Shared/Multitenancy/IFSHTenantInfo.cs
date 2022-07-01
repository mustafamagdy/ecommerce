using Finbuckle.MultiTenant;

namespace FSH.WebApi.Shared.Multitenancy;

public interface IFSHTenantInfo : ITenantInfo
{
  public SubscriptionType SubscriptionType { get; set; }
}