﻿using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Identity;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Authorization;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

internal sealed class ApplicationDbSeeder
{
  private readonly FSHTenantInfo _currentTenant;
  private readonly RoleManager<ApplicationRole> _roleManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly CustomSeederRunner _seederRunner;
  private readonly ILogger<ApplicationDbSeeder> _logger;

  public ApplicationDbSeeder(FSHTenantInfo currentTenant, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
  {
    _currentTenant = currentTenant;
    _roleManager = roleManager;
    _userManager = userManager;
    _seederRunner = seederRunner;
    _logger = logger;
  }

  public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
  {
    await SeedRolesAsync(dbContext);
    await SeedAdminUserAsync();
    await _seederRunner.RunSeedersAsync(cancellationToken);
  }

  private async Task SeedRolesAsync(ApplicationDbContext dbContext)
  {
    foreach (string roleName in FSHRoles.DefaultRoles)
    {
      if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName) is not { } role)
      {
        // Create the role
        _logger.LogDebug("Seeding {Role} Role for '{TenantId}' Tenant", roleName, _currentTenant.Id);
        role = new ApplicationRole(roleName, $"{roleName} Role for {_currentTenant.Id} Tenant");
        await _roleManager.CreateAsync(role);
      }

      switch (roleName)
      {
        // Assign permissions
        case FSHRoles.Admin:
          await AssignPermissionsToRoleAsync(dbContext, FSHPermissions.Admin, role);

          if (_currentTenant.Id == MultitenancyConstants.Root.Id)
          {
            await AssignPermissionsToRoleAsync(dbContext, FSHPermissions.Root, role);
          }

          break;
      }
    }
  }

  private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyCollection<FSHPermission> permissions, ApplicationRole role)
  {
    var currentClaims = await _roleManager.GetClaimsAsync(role);
    foreach (var permission in permissions)
    {
      if (currentClaims.Any(c => c.Type == FSHClaims.Permission && c.Value == permission.Name))
      {
        continue;
      }

      _logger.LogDebug("Seeding {Role} Permission '{Permission}' for '{TenantId}' Tenant", role.Name, permission.Name, _currentTenant.Id);
      dbContext.RoleClaims.Add(new ApplicationRoleClaim
      {
        RoleId = role.Id,
        ClaimType = FSHClaims.Permission,
        ClaimValue = permission.Name,
        CreatedBy = "ApplicationDbSeeder"
      });
      await dbContext.SaveChangesAsync();
    }
  }

  private async Task SeedAdminUserAsync()
  {
    if (string.IsNullOrWhiteSpace(_currentTenant.Id) || string.IsNullOrWhiteSpace(_currentTenant.AdminEmail))
    {
      return;
    }

    if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == _currentTenant.AdminEmail)
        is not { } adminUser)
    {
      string adminUserName = $"{_currentTenant.Id.Trim()}.{FSHRoles.Admin}".ToLowerInvariant();
      adminUser = new ApplicationUser
      {
        FirstName = _currentTenant.Id.Trim().ToLowerInvariant(),
        LastName = FSHRoles.Admin,
        Email = _currentTenant.AdminEmail,
        UserName = adminUserName,
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        NormalizedEmail = _currentTenant.AdminEmail?.ToUpperInvariant(),
        NormalizedUserName = adminUserName.ToUpperInvariant(),
        Active = true
      };

      _logger.LogDebug("Seeding Default Admin User for '{TenantId}' Tenant", _currentTenant.Id);
      var password = new PasswordHasher<ApplicationUser>();
      adminUser.PasswordHash = password.HashPassword(adminUser, MultitenancyConstants.DefaultPassword);
      await _userManager.CreateAsync(adminUser);
    }

    // Assign role to user
    if (!await _userManager.IsInRoleAsync(adminUser, FSHRoles.Admin))
    {
      _logger.LogInformation("Assigning Admin Role to Admin User for '{TenantId}' Tenant", _currentTenant.Id);
      await _userManager.AddToRoleAsync(adminUser, FSHRoles.Admin);
    }
  }
}