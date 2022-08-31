using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Multitenancy.Services;

public interface ITenantService
{
  Task<bool> ExistsWithIdAsync(string id);
  Task<bool> ExistsWithNameAsync(string name);
}