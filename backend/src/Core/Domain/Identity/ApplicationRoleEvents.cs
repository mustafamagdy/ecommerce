namespace FSH.WebApi.Domain.Identity;

public abstract class ApplicationRoleEvent : DomainEvent
{
  public string RoleId { get; private set; }
  public string RoleName { get; private set; }

  protected ApplicationRoleEvent(string roleId, string roleName) =>
    (RoleId, RoleName) = (roleId, roleName);
}

public sealed class ApplicationRoleCreatedEvent : ApplicationRoleEvent
{
  public ApplicationRoleCreatedEvent(string roleId, string roleName)
    : base(roleId, roleName)
  {
  }
}

public sealed class ApplicationRoleUpdatedEvent : ApplicationRoleEvent
{
  public bool PermissionsUpdated { get; private set; }

  public ApplicationRoleUpdatedEvent(string roleId, string roleName, bool permissionsUpdated = false)
    : base(roleId, roleName) =>
    PermissionsUpdated = permissionsUpdated;
}

public sealed class ApplicationRoleDeletedEvent : ApplicationRoleEvent
{
  public bool PermissionsUpdated { get; private set; }

  public ApplicationRoleDeletedEvent(string roleId, string roleName)
    : base(roleId, roleName)
  {
  }
}