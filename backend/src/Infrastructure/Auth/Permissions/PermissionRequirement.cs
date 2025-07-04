using Microsoft.AspNetCore.Authorization;

namespace FSH.WebApi.Infrastructure.Auth.Permissions;

internal sealed class PermissionRequirement : IAuthorizationRequirement
{
  public string Permission { get; private set; }

  public PermissionRequirement(string permission)
  {
    Permission = permission;
  }
}