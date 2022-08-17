using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy.Services;

public interface ITenantConnectionStringResolver : ITransientService
{
  string Resolve(string tenantId, SubscriptionType subscriptionType);
}