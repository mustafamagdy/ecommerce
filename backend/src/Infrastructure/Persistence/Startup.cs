using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.ConnectionString;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using FSH.WebApi.Infrastructure.Persistence.Repository;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;

namespace FSH.WebApi.Infrastructure.Persistence;

internal static class Startup
{
  private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

  internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
  {
    services.AddOptions<DatabaseSettings>()
      .BindConfiguration(nameof(DatabaseSettings))
      .PostConfigure(databaseSettings =>
      {
        _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
      })
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddScoped<DomainEventDispatcher>();
    services.AddScoped<FixNpgDateTimeKind>();
    return services
      .AddDbContext<ApplicationDbContext>((sp, dbOptions) =>
      {
        var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        if (string.Equals(databaseSettings.DBProvider, DbProviderKeys.Npgsql, StringComparison.CurrentCultureIgnoreCase))
        {
          dbOptions.AddInterceptors(sp.GetService<FixNpgDateTimeKind>() ?? throw new NotSupportedException("Fix database datetime kind for postgres not registered"));
        }

        dbOptions.AddInterceptors(sp.GetService<DomainEventDispatcher>() ?? throw new NotSupportedException("Domain dispatcher not registered"));

        dbOptions.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
      })
      .AddApplicationUnitOfWork()
      .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
      .AddTransient<ApplicationDbInitializer>()
      .AddTransient<ApplicationDbSeeder>()
      .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
      .AddTransient<CustomSeederRunner>()
      .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
      .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
      .AddRepositories()
      .AddTransient<ITenantSequenceGenerator>(sp =>
      {
        var databaseSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        var currentTenant = sp.GetService<ITenantInfo>();
        var env = sp.GetService<IHostEnvironment>();
        var counterRepo = sp.GetService<IDapperEntityRepository>();

        return databaseSettings.DBProvider.ToLower() switch
        {
          DbProviderKeys.MySql => new MySqlTenantSequenceGenerator(config, currentTenant, env, counterRepo),
          DbProviderKeys.Npgsql => new NpgsqlTenantSequenceGenerator(config, currentTenant, env, counterRepo),
          _ => throw new NotSupportedException($"Provider {databaseSettings.DBProvider} doesn't have a sequence generator")
        };
      });
  }

  private static IServiceCollection AddApplicationUnitOfWork(this IServiceCollection services)
    => services
      .AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>()
      .AddScoped<ApplicationUnitOfWork>();

  internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
  {
    switch (dbProvider.ToLowerInvariant())
    {
      case DbProviderKeys.Npgsql:
        return builder.UseNpgsql(connectionString, e =>
          e.MigrationsAssembly("Migrators.PostgreSQL"));

      case DbProviderKeys.SqlServer:
        return builder.UseSqlServer(connectionString, e =>
          e.MigrationsAssembly("Migrators.MSSQL"));

      case DbProviderKeys.MySql:
        return builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), e =>
          e.MigrationsAssembly("Migrators.MySQL")
            .SchemaBehavior(MySqlSchemaBehavior.Ignore));

      // case DbProviderKeys.Oracle:
      //   return builder.UseOracle(connectionString, e =>
      //     e.MigrationsAssembly("Migrators.Oracle"));

      case DbProviderKeys.SqLite:
        return builder.UseSqlite(connectionString, e =>
          e.MigrationsAssembly("Migrators.SqLite"));

      default:
        throw new InvalidOperationException($"DB Provider {dbProvider} is not supported.");
    }
  }

  private static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    // Add Repositories
    services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));
    services.AddScoped(typeof(INonAggregateRepository<>), typeof(NonAggregateDbRepository<>));
    services.AddScoped(typeof(IReadNonAggregateRepository<>), typeof(NonAggregateDbRepository<>));

    var aggregateRootTypes = typeof(IAggregateRoot)
      .Assembly
      .GetExportedTypes()
      .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
      .ToList();

    foreach (var aggregateRootType in aggregateRootTypes)
    {
      // Add ReadRepositories.
      services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
        sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

      // Decorate the repositories with EventAddingRepositoryDecorators and expose them as IRepositoryWithEvents.
      services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
        Activator.CreateInstance(
          typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
          sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
        ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for aggregateRootType {aggregateRootType.Name}"));
    }

    return services;
  }
}