namespace FSH.WebApi.Infrastructure.Auth;

public sealed class SecuritySettings
{
  public string? Provider { get; set; }
  public bool RequireActiveTenantSubscription { get; set; }
}