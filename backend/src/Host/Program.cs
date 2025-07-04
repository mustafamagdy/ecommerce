using System.Reflection;
using FSH.WebApi.Application;
using FSH.WebApi.Host.Configurations;
using FSH.WebApi.Host.Controllers;
using FSH.WebApi.Infrastructure;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Registrations;
using QuestPDF.Drawing;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

[assembly: ApiConventionType(typeof(FSHApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
  var builder = WebApplication.CreateBuilder(args);

  builder.Host.AddConfigurations();
  builder.Host.UseSerilog((_, config) =>
  {
    config.ReadFrom.Configuration(builder.Configuration);
  });

  builder.Services
    .AddApplicationControllers()
    .AddApplicationJsonOptions();

  builder.Services.AddInfrastructure(builder.Configuration);
  builder.Services.AddApplication();

  var app = builder.Build();

  await app.Services.InitializeDatabasesAsync();

  var env = app.Environment.EnvironmentName;
  if (!env.Contains("test"))
  {
    string? appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    FontManager.RegisterFont(File.OpenRead(appPath + "/Files/fonts/LibreBarcode39-Regular.ttf"));
  }

  app.UseInfrastructure(builder.Configuration);
  app.MapEndpoints();

  await app.RunAsync();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
  StaticLogger.EnsureInitialized();
  Log.Fatal(ex, "Unhandled exception");
} finally
{
  StaticLogger.EnsureInitialized();
  Log.Information("Server Shutting down...");
  Log.CloseAndFlush();
}

public partial class Program
{
  // Only anchor for integration tests
}