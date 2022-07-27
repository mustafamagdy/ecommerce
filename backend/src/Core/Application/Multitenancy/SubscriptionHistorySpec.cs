using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public class SubscriptionHistorySpec : Specification<SubscriptionHistory>
{
  public SubscriptionHistorySpec(string tenantId) =>
    Query
      .Include(a => a.TenantProdSubscription)
      .Where(a => a.TenantProdSubscription.TenantId == tenantId);
}