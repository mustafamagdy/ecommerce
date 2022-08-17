using System.Data.Common;
using Ardalis.Specification;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Common.Extensions;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Infrastructure.Multitenancy;

internal class TenantService : ITenantService
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

  public async Task<bool> DatabaseExistAsync(string databaseName)
  {
    return (await _tenantStore.GetAllAsync()).Any(t =>
    {
      if (string.IsNullOrEmpty(t.ConnectionString)) return false;
      var cnBuilder = new DbConnectionStringBuilder
      {
        ConnectionString = t.ConnectionString
      };

      var newDbName = $"{_env.GetShortenName()}-{databaseName}";
      return cnBuilder.TryGetValue("initial catalog", out var dbName) && newDbName.Equals(dbName);
    });
  }
}