using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class HostFixture : IDisposable
{
  private readonly WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();

  public HostFixture()
  {
    var dbName = "multi-tenant-09";
    _factory = new TestWebApplicationFactory()
        // .WithWebHostBuilder(builder =>
        // {
        //   builder.ConfigureAppConfiguration((context, configBuilder) =>
        //   {
        //     configBuilder.AddInMemoryCollection(
        //       new Dictionary<string, string>
        //       {
        //         ["DatabaseSettings:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={dbName};User Id=root;Password=DeV12345",
        //         ["HangfireSettings:Storage:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={dbName};User Id=root;Password=DeV12345;Allow User Variables=true"
        //       });
        //     // var path = AppDomain.CurrentDomain.BaseDirectory;
        //     // configBuilder.AddJsonFile($"{path}/tests-appsettings.json", optional: false, reloadOnChange: true);
        //   });
        // })
      ;
  }

  public HttpClient CreateClient() => _factory.CreateClient();

  public void Dispose()
  {
    _factory.Dispose();
  }
}