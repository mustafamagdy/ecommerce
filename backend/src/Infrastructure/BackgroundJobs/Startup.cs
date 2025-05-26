using FSH.WebApi.Infrastructure.Common;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.MySql;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace FSH.WebApi.Infrastructure.BackgroundJobs;

internal static class Startup
{
  private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

  internal static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration config)
  {
    services.AddHangfireServer(options => config.GetSection("HangfireSettings:Server").Bind(options));

    services.AddHangfireConsoleExtensions();

    var storageSettings = config.GetSection("HangfireSettings:Storage").Get<HangfireStorageSettings>();

    var storageProvider = string.IsNullOrEmpty(storageSettings.StorageProvider)
      ? config["DatabaseSettings:DBProvider"]
      : storageSettings.StorageProvider;

    if (string.IsNullOrEmpty(storageProvider))
      throw new Exception("Hangfire Storage Provider is not configured.");

    var connectionString = string.IsNullOrEmpty(storageSettings.ConnectionString)
      ? config["DatabaseSettings:ConnectionString"]
      : storageSettings.ConnectionString;

    if (string.IsNullOrEmpty(connectionString))
      throw new Exception("Hangfire Storage Provider ConnectionString is not configured.");
    _logger.Information($"Hangfire: Current Storage Provider : {storageProvider}");

    services.AddSingleton<JobActivator, FSHJobActivator>();

    services.AddHangfire((provider, hangfireConfig) =>
      hangfireConfig
        .UseDatabase(storageProvider, connectionString, config)
        .UseFilter(new FSHJobFilter(provider))
        .UseFilter(new LogJobFilter())
        .UseColouredConsoleLogProvider());

    return services;
  }

  internal static IGlobalConfiguration UseDatabase(this IGlobalConfiguration hangfireConfig, string dbProvider,
    string connectionString, IConfiguration config) =>
    dbProvider.ToLowerInvariant() switch
    {
      DbProviderKeys.Npgsql =>
        hangfireConfig.UsePostgreSqlStorage(connectionString,
          config.GetSection("HangfireSettings:Storage:Options").Get<PostgreSqlStorageOptions>()),
      DbProviderKeys.SqlServer =>
        hangfireConfig.UseSqlServerStorage(connectionString,
          config.GetSection("HangfireSettings:Storage:Options").Get<SqlServerStorageOptions>()),
      DbProviderKeys.SqLite =>
        hangfireConfig.UseSQLiteStorage(connectionString,
          config.GetSection("HangfireSettings:Storage:Options").Get<SQLiteStorageOptions>()),
      DbProviderKeys.MySql =>
        hangfireConfig.UseStorage(new MySqlStorage(connectionString,
          config.GetSection("HangfireSettings:Storage:Options").Get<MySqlStorageOptions>())),
      _ => throw new Exception($"Hangfire Storage Provider {dbProvider} is not supported.")
    };

  internal static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app, IConfiguration config)
  {
    var dashboardOptions = config.GetSection("HangfireSettings:Dashboard").Get<DashboardOptions>();

    dashboardOptions.Authorization = new[]
    {
      new HangfireCustomBasicAuthenticationFilter
      {
        User = config.GetSection("HangfireSettings:Credentials:User").Value,
        Pass = config.GetSection("HangfireSettings:Credentials:Password").Value
      }
    };

    return app.UseHangfireDashboard(config["HangfireSettings:Route"], dashboardOptions);
  }
}