using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Dashboard;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Users;

namespace FSH.WebApi.Host.Controllers.DemoPermissions;

public class Demo1Controller : VersionedApiController
{
  private readonly IUserService _userService;
  private readonly IRoleService _roleService;

  public Demo1Controller(IUserService userService, IRoleService roleService)
  {
    _userService = userService;
    _roleService = roleService;
  }

  [HttpGet("{username}")]
  [MustHavePermission(FSHAction.View, FSHResource.Brands)]
  [OpenApiOperation("Setup the permissions demo 1", "")]
  public async Task<dynamic> Setup(string username)
  {
    var roleName = "user";
    var password = "1234@1234";
    await _userService.CreateAsync(
      new CreateUserRequest
      {
        UserName = username,
        Email = $"{username}@root.com",
        Password = password
      }, "https://localhost:5001/");

    var user = (await _userService.GetListAsync(CancellationToken.None)).FirstOrDefault(s => s.UserName == username);
// _userService.conf
    var role = (await _roleService.GetListAsync(CancellationToken.None)).FirstOrDefault(a => a.Name == roleName);
    if (role == null)
    {
      await _roleService.CreateOrUpdateAsync(new CreateOrUpdateRoleRequest
      {
        Name = roleName
      });
      role = (await _roleService.GetListAsync(CancellationToken.None)).FirstOrDefault(a => a.Name == roleName);
    }

    await _userService.AssignRolesAsync(user.Id.ToString(), new UserRolesRequest
    {
      UserRoles = new List<UserRoleDto>()
      {
        new() { RoleId = role.Id, Enabled = true, RoleName = role.Name }
      }
    }, CancellationToken.None);

    return new
    {
      user.Email,
      password
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