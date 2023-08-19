using System.Reflection;
using FSH.WebApi.Application.Mappings;
using FSH.WebApi.Application.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Application;

public static class Startup
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    DtoCustomMapping.Configure();

    var assembly = Assembly.GetExecutingAssembly();
    return services
      .AddValidatorsFromAssembly(assembly)
      .AddMediatR(m =>
      {
        m.RegisterServicesFromAssembly(assembly);
      });
  }
}