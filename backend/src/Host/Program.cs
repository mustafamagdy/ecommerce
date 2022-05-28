using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using FSH.WebApi.Application;
using FSH.WebApi.Host.Configurations;
using FSH.WebApi.Host.Controllers;
using FSH.WebApi.Infrastructure;
using FSH.WebApi.Infrastructure.Common;
using FSH.WebApi.Infrastructure.Multitenancy;
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
      opt.Filters.Add<HasValidSubscriptionLevelFilter>();
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

  app.UseInfrastructure(builder.Configuration);
  app.MapEndpoints();
  app.Run();
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