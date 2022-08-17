using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy;

public interface ITenantService
{
  Task<bool> ExistsWithIdAsync(string id);
  Task<bool> ExistsWithNameAsync(string name);
  Task<TenantDto> GetByIdAsync(string id);
  Task<FSHTenantInfo> GetTenantById(string id);
  Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken);
  Task<bool> DatabaseExistAsync(string databaseName);
  Task<bool> HasAValidProdSubscription(string tenantId);

  Task<T> GetSubscription<T>(SubscriptionType subscriptionType)
    where T : Subscription;
}