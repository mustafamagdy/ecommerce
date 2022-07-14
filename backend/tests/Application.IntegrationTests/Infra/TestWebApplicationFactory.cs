using FSH.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Application.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override IHost CreateHost(IHostBuilder builder)
  {
    var dbName = "multi-tenant-10";

    builder.ConfigureAppConfiguration(config =>
      config.AddInMemoryCollection(new Dictionary<string, string>
      {
        ["DatabaseSettings:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={dbName};User Id=root;Password=DeV12345",
        ["HangfireSettings:Storage:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={dbName};User Id=root;Password=DeV12345;Allow User Variables=true"
      })
    );

    return base.CreateHost(builder);
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
      services.Replace(ServiceDescriptor.Transient<ISystemTime>(sp => HostFixture.SYSTEM_TIME));
    });
  }
}