using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
  private readonly TenantDbContext _tenantDbContext;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<DatabaseInitializer> _logger;

  public DatabaseInitializer(TenantDbContext tenantDbContext, IServiceProvider serviceProvider,
    ILogger<DatabaseInitializer> logger)
  {
    _tenantDbContext = tenantDbContext;
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
  {
    await InitializeTenantDbAsync(cancellationToken);

    foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
    {
      await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
    }
  }

  public async Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken)
  {
    // First create a new scope
    using var scope = _serviceProvider.CreateScope();

    // Then set current tenant so the right connectionstring is used
    _serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
      .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
    {
      TenantInfo = tenant
    };

    // initialize per connection string ?? (prod, demo, train)
    // Then run the initialization in the new scope
    await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
      .InitializeAsync(cancellationToken);
  }

  private async Task InitializeTenantDbAsync(CancellationToken cancellationToken)
  {
    if ((await _tenantDbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
    {
      _logger.LogInformation("Applying Root Migrations");
      await _tenantDbContext.Database.MigrateAsync(cancellationToken);
    }

    if (await _tenantDbContext.Database.CanConnectAsync(cancellationToken))
    {
      _logger.LogInformation("Connection to main tenant database succeeded");
      await SeedRootTenantAsync(cancellationToken);
    }
  }

  private async Task SeedRootTenantAsync(CancellationToken cancellationToken)
  {
    if (await _tenantDbContext.TenantInfo.FindAsync(new object?[] { MultitenancyConstants.Root.Id },
          cancellationToken: cancellationToken) is null)
    {
      var rootTenant = new FSHTenantInfo(
        MultitenancyConstants.Root.Id,
        MultitenancyConstants.Root.Name,
        string.Empty,
        string.Empty,
        string.Empty,
        MultitenancyConstants.Root.EmailAddress,
        null, null, null, null, null, null, null);

      _tenantDbContext.TenantInfo.Add(rootTenant);

      await _tenantDbContext.SaveChangesAsync(cancellationToken);
    }
  }
}