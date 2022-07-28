using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy;

public interface ITenantService
{
  Task<List<TenantDto>> GetAllAsync();
  Task<bool> ExistsWithIdAsync(string id);
  Task<bool> ExistsWithNameAsync(string name);
  Task<TenantDto> GetByIdAsync(string id);
  Task<FSHTenantInfo> GetTenantById(string id);
  Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken);
  Task<string> ActivateAsync(string tenantId);
  Task<string> DeactivateAsync(string tenantId);
  Task<string> RenewSubscription(FSHTenantInfo tenant, Subscription subscription);
  Task<bool> DatabaseExistAsync(string databaseName);
  Task<BasicTenantInfoDto> GetBasicInfoByIdAsync(string tenantId);
  Task<bool> HasAValidProdSubscription(string tenantId);
  Task<Unit> PayForSubscription(Guid subscriptionId, decimal amount, Guid? paymentMethodId);
  Task<T> GetSubscription<T>(SubscriptionType subscriptionType)
    where T : Subscription;
}