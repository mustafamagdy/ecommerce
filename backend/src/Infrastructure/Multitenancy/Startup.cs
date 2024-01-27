using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Persistence;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace FSH.WebApi.Infrastructure.Multitenancy;

internal static class Startup
{
  internal static IServiceCollection AddMultitenancy(this IServiceCollection services, IConfiguration config)
  {
    return services
      .AddDbContext<TenantDbContext>((sp, dbOptions) =>
      {
        var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        if (string.Equals(databaseSettings.DBProvider, DbProviderKeys.Npgsql,
              StringComparison.CurrentCultureIgnoreCase))
        if (string.Equals(databaseSettings.DBProvider, DbProviderKeys.Npgsql,
              StringComparison.CurrentCultureIgnoreCase))
        {
          dbOptions.AddInterceptors(sp.GetService<FixNpgDateTimeKind>() ??
                                    throw new NotSupportedException(
                                      "Fix database datetime kind for postgres not registered"));
          dbOptions.AddInterceptors(sp.GetService<FixNpgDateTimeKind>() ??
                                    throw new NotSupportedException(
                                      "Fix database datetime kind for postgres not registered"));
        }

        dbOptions.AddInterceptors(sp.GetService<DomainEventDispatcher>() ??
                                  throw new NotSupportedException("Domain dispatcher not registered"));
        dbOptions.AddInterceptors(sp.GetService<DomainEventDispatcher>() ??
                                  throw new NotSupportedException("Domain dispatcher not registered"));
        dbOptions.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
      })
      .AddTenantUnitOfWork()
      .AddMultiTenant<FSHTenantInfo>()
        .WithHostStrategy()
        .WithHeaderStrategy(MultitenancyConstants.TenantIdName)
        .WithClaimStrategy(FSHClaims.Tenant)
        .WithCustomEFCoreStore<TenantDbContext, FSHTenantInfo>()
      .Services
      .AddScoped<ISubscriptionTypeResolver, SubscriptionTypeResolver>()
      .AddScoped<ITenantService, TenantService>()
      .AddSingleton<ITenantConnectionStringBuilder, TenantConnectionStringBuilder>();
  }

  private static FinbuckleMultiTenantBuilder<TTenantInfo> WithCustomEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
    where TEFCoreStoreDbContext : EFCoreStoreDbContext<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
  {
    builder.Services.AddDbContext<TEFCoreStoreDbContext>();
    return builder.WithStore<EFCoreCustomStore<TEFCoreStoreDbContext, TTenantInfo>>(ServiceLifetime.Scoped);
  }

  private static IServiceCollection AddTenantUnitOfWork(this IServiceCollection services)
    => services
      .AddScoped<ITenantUnitOfWork, TenantUnitOfWork>()
      .AddScoped<TenantUnitOfWork>();

  internal static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
    app.UseMultiTenant();

  private static FinbuckleMultiTenantBuilder<FSHTenantInfo> WithQueryStringStrategy(
    this FinbuckleMultiTenantBuilder<FSHTenantInfo> builder, string queryStringKey) =>
    builder.WithDelegateStrategy(context =>
    {
      if (context is not HttpContext httpContext)
      {
        return Task.FromResult((string?)null);
      }

      httpContext.Request.Query.TryGetValue(queryStringKey, out StringValues tenantIdParam);

      return Task.FromResult((string?)tenantIdParam.ToString());
    });
}