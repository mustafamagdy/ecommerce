using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StackExchange.Redis;

namespace FSH.WebApi.Infrastructure.Identity;

internal sealed class RoleService : IRoleService
{
  private readonly RoleManager<ApplicationRole> _roleManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IStringLocalizer _t;
  private readonly ICurrentUser _currentUser;
  private readonly ITenantInfo _currentTenant;
  private readonly IEventPublisher _events;
  private readonly IApplicationUnitOfWork _uow;

  public RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IStringLocalizer<RoleService> localizer,
    ICurrentUser currentUser,
    ITenantInfo currentTenant,
    IEventPublisher events, IApplicationUnitOfWork uow)
  {
    _roleManager = roleManager;
    _userManager = userManager;
    _t = localizer;
    _currentUser = currentUser;
    _currentTenant = currentTenant;
    _events = events;
    _uow = uow;
  }

  public async Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken) =>
    (await _roleManager.Roles.ToListAsync(cancellationToken))
    .Adapt<List<RoleDto>>();

  public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
    await _roleManager.Roles.CountAsync(cancellationToken);

  public async Task<bool> ExistsAsync(string roleName, string? excludeId) =>
    await _roleManager.FindByNameAsync(roleName)
      is ApplicationRole existingRole
    && existingRole.Id != excludeId;

  public async Task<RoleDto> GetByIdAsync(string id) =>
    await _uow.Set<ApplicationRole>().SingleOrDefaultAsync(x => x.Id == id) is { } role
      ? role.Adapt<RoleDto>()
      : throw new NotFoundException(_t["Role Not Found"]);

  public async Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken)
  {
    var role = await GetByIdAsync(roleId);

    role.Permissions = await _uow.Set<ApplicationRoleClaim>()
      .Where(c => c.RoleId == roleId && c.ClaimType == FSHClaims.Permission)
      .Select(c => c.ClaimValue)
      .ToListAsync(cancellationToken);

    return role;
  }

  public async Task<RoleDto> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request)
  {
    if (string.IsNullOrEmpty(request.Id))
    {
      // Create a new role.
      var role = new ApplicationRole(request.Name, request.Description);
      var result = await _roleManager.CreateAsync(role);

      if (!result.Succeeded)
      {
        throw new InternalServerException(_t["Register role failed"], result.GetErrors(_t));
      }

      await _events.PublishAsync(new ApplicationRoleCreatedEvent(role.Id, role.Name));

      return role.Adapt<RoleDto>();
    }
    else
    {
      // Update an existing role.
      var role = await _roleManager.FindByIdAsync(request.Id);

      _ = role ?? throw new NotFoundException(_t["Role Not Found"]);

      if (FSHRoles.IsDefault(role.Name))
      {
        throw new ConflictException(string.Format(_t["Not allowed to modify {0} Role."], role.Name));
      }

      role.Name = request.Name;
      role.NormalizedName = request.Name.ToUpperInvariant();
      role.Description = request.Description;
      var result = await _roleManager.UpdateAsync(role);

      if (!result.Succeeded)
      {
        throw new InternalServerException(_t["Update role failed"], result.GetErrors(_t));
      }

      await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name));

      return role.Adapt<RoleDto>();
    }
  }

  public async Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
  {
    var role = await _roleManager.FindByIdAsync(request.RoleId);
    _ = role ?? throw new NotFoundException(_t["Role Not Found"]);
    if (role.Name == FSHRoles.Admin)
    {
      throw new ConflictException(_t["Not allowed to modify Permissions for this Role."]);
    }

    if (_currentTenant.Id != MultitenancyConstants.Root.Id)
    {
      // Remove Root Permissions if the Role is not created for Root Tenant.
      request.Permissions.RemoveAll(u => u.StartsWith("Permissions.Root."));
    }

    var currentClaims = await _roleManager.GetClaimsAsync(role);

    // Remove permissions that were previously selected
    foreach (var claim in currentClaims.Where(c => !request.Permissions.Any(p => p == c.Value)))
    {
      var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
      if (!removeResult.Succeeded)
      {
        throw new InternalServerException(_t["Update permissions failed."], removeResult.GetErrors(_t));
      }
    }

    // Add all permissions that were not previously selected
    foreach (string permission in request.Permissions.Where(c => !currentClaims.Any(p => p.Value == c)))
    {
      if (!string.IsNullOrEmpty(permission))
      {
        _uow.Set<ApplicationRoleClaim>().Add(new ApplicationRoleClaim
        {
          RoleId = role.Id,
          ClaimType = FSHClaims.Permission,
          ClaimValue = permission,
          CreatedBy = _currentUser.GetUserId().ToString()
        });
        await _uow.CommitAsync(cancellationToken);
      }
    }

    await _events.PublishAsync(new ApplicationRoleUpdatedEvent(role.Id, role.Name, true));

    return _t["Permissions Updated."];
  }

  public async Task<string> DeleteRole(string id)
  {
    var role = await _roleManager.FindByIdAsync(id);

    _ = role ?? throw new NotFoundException(_t["Role Not Found"]);

    if (FSHRoles.IsDefault(role.Name))
    {
      throw new ConflictException(string.Format(_t["Not allowed to delete {0} Role."], role.Name));
    }

    var roleUsers = await _userManager.GetUsersInRoleAsync(role.Name);
    if (roleUsers.Count > 0)
    {
      foreach (var user in roleUsers)
      {
        await _userManager.RemoveFromRoleAsync(user, role.Name);
      }
    }

    await _roleManager.DeleteAsync(role);

    await _events.PublishAsync(new ApplicationRoleDeletedEvent(role.Id, role.Name));

    return string.Format(_t["Role {0} Deleted."], role.Name);
  }
}