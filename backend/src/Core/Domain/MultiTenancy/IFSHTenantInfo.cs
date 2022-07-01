using Finbuckle.MultiTenant;

namespace FSH.WebApi.Domain.MultiTenancy;

public interface IFSHTenantInfo : ITenantInfo
{
  public SubscriptionType SubscriptionType { get; set; }
}