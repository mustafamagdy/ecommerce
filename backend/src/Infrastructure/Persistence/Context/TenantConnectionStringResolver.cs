using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Infrastructure.Persistence.Context;


public class TenantConnectionStringResolver : ITenantConnectionStringResolver
{
  private readonly TenantDbContext _tenantDb;

  public TenantConnectionStringResolver(TenantDbContext tenantDb)
  {
    _tenantDb = tenantDb;
  }

  public string Resolve(string tenantId, SubscriptionType subscriptionType)
  {
    var tenant = _tenantDb.TenantInfo.Find(tenantId);
    return subscriptionType.Name switch
    {
      nameof(SubscriptionType.Standard) => tenant.ConnectionString,
      nameof(SubscriptionType.Demo) => tenant.DemoConnectionString,
      nameof(SubscriptionType.Train) => tenant.TrainConnectionString,
      _ => throw new ArgumentOutOfRangeException(subscriptionType.Name)
    };
  }
}