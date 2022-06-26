using Finbuckle.MultiTenant;

namespace FSH.WebApi.Shared.Multitenancy;

public class MultitenancyConstants
{
  private static readonly RootTenantInfo _rootTenant = new();
  public const string JobRunnerUserName = "JobRunner";

  public static class Root
  {
    public const string Id = "root";
    public const string Name = "Root";
    public const string EmailAddress = "admin@root.com";
  }

  public static RootTenantInfo RootTenant { get => _rootTenant; }

  public class RootTenantInfo : ITenantInfo
  {
    public string? Id { get => Root.Id; set => throw new InvalidOperationException("You cannot set root tenant Id"); }

    public string? Identifier { get => Root.Id; set => throw new InvalidOperationException("You cannot set root tenant Id"); }
    public string? Name { get => Root.Name; set => throw new InvalidOperationException("You cannot set root tenant Name"); }
    public string? ConnectionString { get => string.Empty; set => throw new NotImplementedException(); }
  }

  public const string DefaultPassword = "123Pa$$word!";

  public const string TenantIdName = "tenant";
}