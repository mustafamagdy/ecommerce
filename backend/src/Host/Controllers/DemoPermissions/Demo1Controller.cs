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

  [HttpGet]
  [MustHavePermission(FSHAction.View, FSHResource.Brands)]
  [OpenApiOperation("Setup the permissions demo 1", "")]
  public async Task Setup()
  {
    var user = await _userService.CreateAsync(
      new CreateUserRequest
      {
        UserName = "demo4",
        Email = "demo4@root.com",
        Password = "1234@1234"
      }, "https://localhost:5001/");

    var role = await _roleService.CreateOrUpdateAsync(new CreateOrUpdateRoleRequest
    {
      Name = "user"
    });

    _userService.AssignRolesAsync(user, new UserRolesRequest
    {
      UserRoles = new List<UserRoleDto>()
      {
        new() { RoleId = role }
      }
    }, CancellationToken.None);
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