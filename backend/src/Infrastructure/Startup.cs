using System.Runtime.CompilerServices;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Infrastructure.Auth;
using FSH.WebApi.Infrastructure.BackgroundJobs;
using FSH.WebApi.Infrastructure.Caching;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Cors;
using FSH.WebApi.Infrastructure.FileStorage;
using FSH.WebApi.Infrastructure.Localization;
using FSH.WebApi.Infrastructure.Mailing;
using FSH.WebApi.Infrastructure.Mapping;
using FSH.WebApi.Infrastructure.Middleware;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Notifications;
using FSH.WebApi.Infrastructure.OpenApi;
using FSH.WebApi.Infrastructure.Persistence;
using FSH.WebApi.Infrastructure.SecurityHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Infrastructure.Test")]
[assembly: InternalsVisibleTo("Application.IntegrationTests")]

namespace FSH.WebApi.Infrastructure;

public static class Startup
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
  {
    MapsterSettings.Configure();

    return services
      .AddApiVersioning()
      .AddAuth(config)
      .AddBackgroundJobs(config)
      .AddCaching(config)
      .AddCorsPolicy(config)
      .AddExceptionMiddleware()
      .AddHealthCheck()
      .AddPOLocalization(config)
      .AddMailing(config)
      .AddMediatR()
      .AddMultitenancy(config)
      .AddNotifications(config)
      .AddOpenApiDocumentation(config)
      .AddPersistence(config)
      .AddRequestLogging(config)
      .AddRouting(options => options.LowercaseUrls = true)
      // .AddWorkflow(config)
      .AddServices()
      .AddHostedServices();
  }

  private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
    services.AddApiVersioning(config =>
    {
      config.DefaultApiVersion = new ApiVersion(1, 0);
      config.AssumeDefaultVersionWhenUnspecified = true;
      config.ReportApiVersions = true;
    });

  private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
    services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

  public static async Task InitializeDatabasesAsync(this IServiceProvider services,
    CancellationToken cancellationToken = default)
  {
    // Create a new scope to retrieve scoped services
    using var scope = services.CreateScope();

    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
      .InitializeDatabasesAsync(cancellationToken);
  }

  public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
    builder
      .UseRequestLocalization()
      .UseStaticFiles()
      .UseSecurityHeaders(config)
      .UseFileStorage()
      .UseExceptionMiddleware()
      .UseRouting()
      .UseCorsPolicy()
      .UseAuthentication()
      .UseCurrentUser()
      .UseMultiTenancy()
      .UseAuthorization()
      .UseRequestLogging(config)
      .UseHangfireDashboard(config)
      .UseOpenApiDocumentation(config);

  public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
  {
    builder.MapControllers().RequireAuthorization();
    builder.MapHealthCheck();
    builder.MapNotifications();
    return builder;
  }

  private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
    endpoints.MapHealthChecks("/api/health").RequireAuthorization();
}