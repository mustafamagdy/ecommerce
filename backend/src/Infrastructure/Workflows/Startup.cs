﻿// using Elsa;
// using Elsa.Persistence.EntityFramework.Core;
// using Elsa.Persistence.EntityFramework.Core.Extensions;
// using FSH.WebApi.Infrastructure.Common;
// using FSH.WebApi.Infrastructure.Persistence;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using FSH.WebApi.Application.Settings;
// using FSH.WebApi.Infrastructure.BackgroundJobs;
// using Hangfire;
// using Hangfire.MySql;
// using Hangfire.PostgreSql;
//
// namespace FSH.WebApi.Infrastructure.Workflows;
//
// internal static class Startup
// {
//   internal static IServiceCollection AddWorkflow(this IServiceCollection services, IConfiguration config)
//   {
//     var settings = config.GetSection(nameof(WorkflowsSettings)).Get<WorkflowsSettings>();
//     var dbSettings = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
//     services.AddElsa(elsa =>
//     {
//       elsa.UseEntityFrameworkPersistence(optionsBuilder =>
//       {
//         optionsBuilder.UseDatabaseForElsa(dbSettings.DBProvider, dbSettings.ConnectionString);
//       }, autoRunMigrations: false);
//
//       elsa.AddConsoleActivities();
//       //TODO: Causing an error on startup, to be investigated
//       // elsa.AddHangfireTemporalActivities(hangfire =>
//       // {
//       //   hangfire.UseDatabase(dbSettings.DBProvider, dbSettings.ConnectionString, config);
//       // });
//
//       elsa.AddWorkflowsFrom<Anchor>();
//     });
//     services
//       .AddDbContext<ElsaContext>(builder => builder.UseDatabaseForElsa(dbSettings.DBProvider, dbSettings.ConnectionString));
//     return services;
//   }
//
//   internal static DbContextOptionsBuilder UseDatabaseForElsa(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
//   {
//     return dbProvider.ToLowerInvariant() switch
//     {
//       DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString, e => e.MigrationsAssembly(typeof(Elsa.Persistence.EntityFramework.PostgreSql.PostgreSqlElsaContextFactory).Assembly.FullName)),
//       DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e => e.MigrationsAssembly(typeof(Elsa.Persistence.EntityFramework.SqlServer.SqlServerElsaContextFactory).Assembly.FullName)),
//       DbProviderKeys.MySql => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), e => e.MigrationsAssembly(typeof(Elsa.Persistence.EntityFramework.MySql.MySqlElsaContextFactory).Assembly.FullName)),
//       _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported for workflow or not configured correctly.")
//     };
//   }
// }