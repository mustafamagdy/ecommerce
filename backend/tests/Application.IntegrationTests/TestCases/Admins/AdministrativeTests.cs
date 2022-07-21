using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Users;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class AdministrativeTests : TestFixture
{
  public AdministrativeTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task admin_of_root_tenant_can_list_all_products()
  {
    _output.WriteLine("Start testing ..");

    var response = await RootAdmin_PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest());
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var productResult = await response.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task admin_can_create_role_and_add_permissions()
  {
    var roleName = Guid.NewGuid().ToString();
    var _ = await RootAdmin_PostAsJsonAsync("/api/roles", new CreateOrUpdateRoleRequest
    {
      Name = roleName,
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var role = await _.Content.ReadFromJsonAsync<RoleDto>();

    //create user
    var password = "Ran60m@pass";
    var username = Guid.NewGuid().ToString();
    var newUser = new CreateUserRequest()
    {
      Email = $"{username}@email.com",
      Password = password,
      ConfirmPassword = password,
      FirstName = username,
      LastName = username,
      UserName = username,
    };
    _ = await RootAdmin_PostAsJsonAsync("/api/users", newUser);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await _.Content.ReadFromJsonAsync<UserDetailsDto>();

    //add user to this role
    _ = await RootAdmin_PostAsJsonAsync($"/api/users/{user.Id}/roles", new UserRolesRequest
    {
      UserRoles = new List<UserRoleDto>
      {
        new()
        {
          RoleId = role.Id,
          Enabled = true
        }
      }
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var roleResult = await _.Content.ReadAsStringAsync();

    //login with the user and execute action => should fail
    var loginHeader = await LoginAs(user.Email, password, null, "root", CancellationToken.None);
    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), loginHeader, CancellationToken.None);
    var productResult = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);

    //update role permissions
    _ = await RootAdmin_PutAsJsonAsync($"/api/roles/{role.Id}/permissions", new UpdateRolePermissionsRequest
    {
      RoleId = role.Id,
      Permissions = new List<string>
      {
        "Permissions.Products.Search"
      }
    });

    _.StatusCode.Should().Be(HttpStatusCode.OK);

    //login again with the user
    loginHeader = await LoginAs(user.Email, password, null, "root", CancellationToken.None);
    //execute the same action => should succeed
    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), loginHeader, CancellationToken.None);
    productResult = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);
  }

  /*
   *
   * admin_can_create_user_with_predefined_role_and_that_user_can_login
   * disabled_user_cannot_login
   * users_cannot_create_themselves_when_this_feature_is_disabled
   * admin_can_reset_any_user_password
   * only_admins_can_reset_admins_password
   * admin_can_change_his_own_password
   * user_can_change_his_password
   * user_can_reset_his_password
   * user_cannot_access_resource_unless_he_has_permission
   *
   */
}