using System.Reflection;
using FSH.WebApi.Application.Mappings;
using FSH.WebApi.Application.MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Application;

public static class Startup
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    DtoCustomMapping.Configure();

    var assembly = Assembly.GetExecutingAssembly();
    return services
      .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>))
      .AddValidatorsFromAssembly(assembly, includeInternalTypes: true)
      .AddMediatR(m =>
      {
        m.RegisterServicesFromAssembly(assembly);
        m.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));
      });
  }
}