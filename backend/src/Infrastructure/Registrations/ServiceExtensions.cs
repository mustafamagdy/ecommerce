using System.Text.Json;
using System.Text.Json.Serialization;
using FSH.WebApi.Infrastructure.Finance;
using FSH.WebApi.Infrastructure.Multitenancy;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Infrastructure.Registrations;

public static class ServiceExtensions
{
  public static IMvcBuilder AddApplicationControllers(this IServiceCollection services)
  {
    return services.AddControllers(opt =>
    {
      opt.Filters.Add<HasValidSubscriptionTypeFilter>();
      opt.Filters.Add<RequireOpenCashRegisterFilter>();
    });
  }

  public static IMvcBuilder AddApplicationJsonOptions(this IMvcBuilder builder)
  {
    return builder.AddJsonOptions(opt =>
    {
      opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      opt.JsonSerializerOptions.Converters.Clear();
      opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    });
  }
}