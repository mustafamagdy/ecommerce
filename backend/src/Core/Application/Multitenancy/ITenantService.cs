using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy;

public interface ITenantService
{
  Task<List<TenantDto>> GetAllAsync();
  Task<bool> ExistsWithIdAsync(string id);
  Task<bool> ExistsWithNameAsync(string name);
  Task<TenantDto> GetByIdAsync(string id);
  Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken);
  Task<string> ActivateAsync(string tenantId);
  Task<string> DeactivateAsync(string tenantId);
  Task<string> RenewSubscription(Guid subscriptionId, DateTime? extendedExpiryDate);
  Task<bool> DatabaseExistAsync(string databaseName);
  Task<BasicTenantInfoDto> GetBasicInfoByIdAsync(string tenantId);
  Task<bool> HasAValidProdSubscription(string tenantId);
}