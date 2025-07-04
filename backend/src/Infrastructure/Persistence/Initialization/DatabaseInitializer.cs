﻿// using Elsa.Persistence.EntityFramework.Core;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

internal sealed class DatabaseInitializer : IDatabaseInitializer
{
  private readonly TenantDbContext _tenantDbContext;
  // private readonly ElsaContext _workflowContext;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<DatabaseInitializer> _logger;

  public DatabaseInitializer(IServiceProvider serviceProvider, TenantDbContext tenantDbContext,
    // ElsaContext workflowContext,
    ILogger<DatabaseInitializer> logger)
  {
    _serviceProvider = serviceProvider;
    _tenantDbContext = tenantDbContext;
    // _workflowContext = workflowContext;
    _logger = logger;
  }

  public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
  {
    await InitializeTenantDbAsync(cancellationToken);
    // await InitializeRootWorkflowDbAsync(cancellationToken);

    foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
    {
      await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
    }
  }

  public async Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken)
  {
    foreach (var subscriptionType in GetSubscriptions(tenant))
    {
      // First create a new scope
      using var scope = _serviceProvider.CreateScope();

      // Then set current tenant so the right connection string is used
      _serviceProvider.GetRequiredService<IMultiTenantContextAccessor>()
        .MultiTenantContext = new MultiTenantContext<FSHTenantInfo>()
      {
        TenantInfo = tenant,
      };

      // initialize per subscription type ?? (prod, demo, train)
      _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext ??= new DefaultHttpContext();
      _serviceProvider.GetRequiredService<IHttpContextAccessor>()
        .HttpContext!
        .Request
        .Headers[MultitenancyConstants.SubscriptionTypeHeaderName] = subscriptionType.Value;

      // Then run the initialization in the new scope
      await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
        .InitializeAsync(cancellationToken);
    }
  }

  private static List<SubscriptionType> GetSubscriptions(FSHTenantInfo tenant)
  {
    var listSubscriptions = new List<SubscriptionType>();
    if (tenant.ProdSubscriptionId != null || tenant.Id == MultitenancyConstants.RootTenant.Id)
    {
      listSubscriptions.Add(SubscriptionType.Standard);
    }

    if (tenant.DemoSubscriptionId != null)
    {
      listSubscriptions.Add(SubscriptionType.Demo);
    }

    if (tenant.TrainSubscriptionId != null)
    {
      listSubscriptions.Add(SubscriptionType.Train);
    }

    return listSubscriptions;
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

  // private async Task InitializeRootWorkflowDbAsync(CancellationToken cancellationToken)
  // {
  //   if ((await _workflowContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
  //   {
  //     _logger.LogInformation("Applying Root Workflow Migrations");
  //     await _workflowContext.Database.MigrateAsync(cancellationToken);
  //   }
  //
  //   if (await _workflowContext.Database.CanConnectAsync(cancellationToken))
  //   {
  //     await SeedRootWorkflowsAsync(cancellationToken);
  //   }
  // }

  private Task SeedRootWorkflowsAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
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