using FSH.WebApi.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Tests;

public class Startup
{
  public void ConfigureHost(IHostBuilder host) =>
    host.ConfigureHostConfiguration(config => config.AddJsonFile("appsettings.json"));

  public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
  {
    services.AddDbContext<ApplicationDbContext>((sp, m) =>
    {
      m.UseInMemoryDatabase(Guid.NewGuid().ToString())
        .UseInternalServiceProvider(sp);
    });
  }
}