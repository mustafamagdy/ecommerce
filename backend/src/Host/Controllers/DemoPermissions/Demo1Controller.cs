using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Dashboard;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Infrastructure.Identity;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Host.Controllers.DemoPermissions;

public sealed class Demo1Controller : VersionedApiController
{
  private readonly RoleManager<ApplicationRole> _roleManager;
  private readonly UserManager<ApplicationUser> _userManager;

  public Demo1Controller(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
  {
    _roleManager = roleManager;
    _userManager = userManager;
  }

  [HttpGet("{username}")]
  [MustHavePermission(FSHAction.View, FSHResource.Brands)]
  [OpenApiOperation("Setup the permissions demo 1", "")]
  public async Task<dynamic> Setup(string username)
  {
    string passwordStr = "1234@1234";
    if (await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username)
        is not { } newUser)
    {
      string email = $"{username}@root.com";
      newUser = new ApplicationUser
      {
        FirstName = username.ToLowerInvariant(),
        LastName = username,
        Email = email,
        UserName = username,
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = username.ToUpperInvariant(),
        IsActive = true
      };

      var password = new PasswordHasher<ApplicationUser>();
      newUser.PasswordHash = password.HashPassword(newUser, passwordStr);
      await _userManager.CreateAsync(newUser);
    }

    // Assign role to user
    if (!await _userManager.IsInRoleAsync(newUser, FSHRoles.Demo))
    {
      await _userManager.AddToRoleAsync(newUser, FSHRoles.Demo);
    }

    return new
    {
      newUser.Email,
      password = passwordStr
    };
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Brands)]
  [OpenApiOperation("Get brand details.", "")]
  public Task<BrandDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetBrandRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Brands)]
  [OpenApiOperation("Create a new brand.", "")]
  public Task<Guid> CreateAsync(CreateBrandRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Brands)]
  [OpenApiOperation("Delete a brand.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteBrandRequest(id));
  }
}