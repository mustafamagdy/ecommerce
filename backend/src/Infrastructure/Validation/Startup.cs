using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Infrastructure.Validation;

internal static class Startup
{
  public static IServiceCollection AddApplicationFluentValidation(this IServiceCollection services)
  {
    // return services.AddFluentValidation(conf =>
    // {
    //   conf.ValidatorFactoryType = typeof(HttpContextServiceProviderValidatorFactory);
    // });
    services.AddSingleton<ICustomValidatorFactory, CustomValidatorFactory>();
    return services.AddFluentValidationAutoValidation(fv => { fv.DisableDataAnnotationsValidation = true; });
  }
}