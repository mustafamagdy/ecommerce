using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Infrastructure.Multitenancy;
using Microsoft.Extensions.DependencyInjection;
using FSH.WebApi.Infrastructure.Persistence.Initialization; // Required for ICustomSeeder
using FSH.WebApi.Infrastructure.Persistence.Initialization.Seeders; // Required for AccountingSeeder
using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Infrastructure.Common;

internal static class Startup
{
  internal static IServiceCollection AddServices(this IServiceCollection services) =>
    services
      .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped)
            .AddTransient<ICustomSeeder, AccountingSeeder>(); // Register AccountingSeeder

  internal static IServiceCollection AddHostedServices(this IServiceCollection services)
  {
    // services.AddHostedService<DemoService>();
    return services;
  }

  internal static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType,
    ServiceLifetime lifetime)
  {
    var interfaceTypes =
      AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName != null && !a.FullName.Contains("Microsoft.EntityFrameworkCore"))
        .SelectMany(s => s.GetTypes())
        .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
        .Select(t => new
        {
          Service = t.GetInterfaces().FirstOrDefault(),
          Implementation = t
        })
        .Where(t => t.Service is not null && interfaceType.IsAssignableFrom(t.Service));

    foreach (var type in interfaceTypes)
    {
      services.AddService(type.Service!, type.Implementation, lifetime);
    }

    return services;
  }

  private static IServiceCollection AddService(this IServiceCollection services, Type serviceType,
    Type implementationType, ServiceLifetime lifetime) =>
    lifetime switch
    {
      ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
      ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
      ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
      _ => throw new ArgumentException("Invalid lifeTime", nameof(lifetime))
    };
}