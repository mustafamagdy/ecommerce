using System.Data.Common;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Infrastructure.Multitenancy;

internal sealed class TenantService : ITenantService
{
  private readonly IMultiTenantStore<FSHTenantInfo> _tenantStore;
  private readonly IConnectionStringSecurer _csSecurer;
  private readonly IHostEnvironment _env;

  public TenantService(
    IMultiTenantStore<FSHTenantInfo> tenantStore,
    IConnectionStringSecurer csSecurer,
    IHostEnvironment env)
  {
    _tenantStore = tenantStore;
    _csSecurer = csSecurer;
    _env = env;
  }

  public async Task<bool> ExistsWithIdAsync(string id) =>
    await _tenantStore.TryGetAsync(id) is not null;

  public async Task<bool> ExistsWithNameAsync(string name) =>
    (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);
}