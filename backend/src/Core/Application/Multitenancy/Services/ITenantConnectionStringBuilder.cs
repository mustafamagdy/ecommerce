using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy.Services;

public interface ITenantConnectionStringBuilder
{
  string BuildConnectionString(string tenantId, SubscriptionType subscriptionType);
}