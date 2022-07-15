using FSH.WebApi.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application.IntegrationTests.Infra;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
  public static string ENV = "test";

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment(ENV);
    builder.ConfigureServices(services =>
    {
      services.Replace(ServiceDescriptor.Transient<ISystemTime>(sp => HostFixture.SYSTEM_TIME));
    });
  }
}