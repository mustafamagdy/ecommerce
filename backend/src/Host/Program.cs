using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using FSH.WebApi.Application;
using FSH.WebApi.Host.Configurations;
using FSH.WebApi.Host.Controllers;
using FSH.WebApi.Infrastructure;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Finance;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Seeders;
using QuestPDF.Drawing;
using Serilog;

[assembly: ApiConventionType(typeof(FSHApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
  var builder = WebApplication.CreateBuilder(args);

  builder.Host.AddConfigurations();
  builder.Host.UseSerilog((_, config) =>
  {
    config.WriteTo.Console()
      .ReadFrom.Configuration(builder.Configuration);
  });

  builder.Services.AddControllers(opt =>
    {
      opt.Filters.Add<HasValidSubscriptionTypeFilter>();
      opt.Filters.Add<RequireOpenCashRegisterFilter>();
    })
    .AddFluentValidation()
    .AddJsonOptions(opt =>
    {
      opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      opt.JsonSerializerOptions.Converters.Clear();
      opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    });

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
  static Dictionary<string, string> s_ConfigOverride { get; set; } = new();

  class ClearConfigOverride : IDisposable
  {
    public void Dispose() => s_ConfigOverride = new Dictionary<string, string>();
  }

  public static IReadOnlyDictionary<string, string> InMemoryConfig => s_ConfigOverride;

  public static IDisposable OverrideConfig(Dictionary<string, string> config)
  {
    s_ConfigOverride = config;
    return new ClearConfigOverride();
  }
}