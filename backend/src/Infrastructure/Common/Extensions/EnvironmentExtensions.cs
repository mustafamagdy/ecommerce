using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Infrastructure.Common.Extensions;

public static class EnvironmentExtensions
{
  public static string GetShortenName(this IHostEnvironment env) => env.EnvironmentName.ToLower() switch
  {
    "production" => "prod",
    "development" => "dev",
    _ => env.EnvironmentName.ToLower(),
  };
}