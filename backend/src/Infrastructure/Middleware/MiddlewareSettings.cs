namespace FSH.WebApi.Infrastructure.Middleware;

public sealed class MiddlewareSettings
{
  public bool EnableHttpsLogging { get; set; } = false;
  public bool EnableLocalization { get; set; } = false;
}