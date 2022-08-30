using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Identity.Users.Password;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;
using netDumbster.smtp;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class AdministrativeTests : TestFixture
{
  public AdministrativeTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
    _output.WriteLine("Start testing ..");
  }

  [Fact]
  public async Task admin_of_root_tenant_can_list_all_products()
  {
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

    // update role permissions
    var newRolePermissionsRequest = new UpdateRolePermissionsRequest
    {
      RoleId = role.Id,
      Permissions = new List<string>
      {
        "Permissions.Products.Search"
      }
    };
    _ = await RootAdmin_PutAsJsonAsync($"/api/roles/{role.Id}/permissions", newRolePermissionsRequest);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await RootAdmin_GetAsync($"/api/roles/{role.Id}/permissions");
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var rolePermissions = await _.Content.ReadFromJsonAsync<RoleDto>();
    rolePermissions.Permissions
      .Should()
      .NotBeNull().And
      .BeEquivalentTo(newRolePermissionsRequest.Permissions);
  }

  [Fact]
  public async Task user_cannot_do_operation_unless_has_the_permission_to_do_it()
  {
    var roleName = Guid.NewGuid().ToString();
    var _ = await RootAdmin_PostAsJsonAsync("/api/roles", new CreateOrUpdateRoleRequest
    {
      Name = roleName,
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var role = await _.Content.ReadFromJsonAsync<RoleDto>();

    // create user
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
    var user = await _.Content.ReadFromJsonAsync<CreateUserResponseDto>();

    // add user to this role
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

    // login with the user and execute action => should fail
    var loginHeader = await LoginAs(user.Email, password, null, "root");
    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), loginHeader, CancellationToken.None);
    var productResult = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);

    // update role permissions
    _ = await RootAdmin_PutAsJsonAsync($"/api/roles/{role.Id}/permissions", new UpdateRolePermissionsRequest
    {
      RoleId = role.Id,
      Permissions = new List<string>
      {
        "Permissions.Products.Search"
      }
    });

    _.StatusCode.Should().Be(HttpStatusCode.OK);

    // login again with the user
    loginHeader = await LoginAs(user.Email, password, null, "root");
    // execute the same action => should succeed
    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), loginHeader, CancellationToken.None);
    productResult = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task admin_can_create_user_with_predefined_role_and_that_user_can_login()
  {
    var _ = await RootAdmin_GetAsync($"/api/roles");
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var roles = await _.Content.ReadFromJsonAsync<List<RoleDto>>();
    roles.Should().NotBeNull().And.NotBeEmpty();
    roles.Should().NotContain(a => a.Permissions != null);
  }

  [Fact]
  public async Task disabled_user_cannot_login()
  {
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
    var _ = await RootAdmin_PostAsJsonAsync("/api/users", newUser);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await _.Content.ReadFromJsonAsync<CreateUserResponseDto>();

    var loginHeader = await LoginAs(user.Email, password, null, "root");
    loginHeader.Should().NotBeNull().And.Contain(a => !string.IsNullOrEmpty(a.Value));

    _ = await RootAdmin_PostAsJsonAsync($"/api/users/{user.Id}/toggle-status", new ToggleUserStatusRequest
    {
      ActivateUser = false,
      UserId = user.Id.ToString()
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(user.Email, password, "root");
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task users_cannot_create_themselves_when_this_feature_is_disabled()
  {
    // Todo: self created user feature disable toggle
  }

  [Fact]
  public async Task admin_can_reset_any_user_password_in_his_tenant()
  {
    var originalPassword = "Ran60m@pass";
    var username = Guid.NewGuid().ToString();
    var newUser = new CreateUserRequest()
    {
      Email = $"{username}@email.com",
      Password = originalPassword,
      ConfirmPassword = originalPassword,
      FirstName = username,
      LastName = username,
      UserName = username,
    };
    var _ = await RootAdmin_PostAsJsonAsync("/api/users", newUser);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await _.Content.ReadFromJsonAsync<CreateUserResponseDto>();

    var loginHeader = await LoginAs(user.Email, originalPassword, null, "root");
    loginHeader.Should().NotBeNull().And.Contain(a => !string.IsNullOrEmpty(a.Value));

    // reset
    var newPassword = "NEW@p@ssword";
    _ = await RootAdmin_PostAsJsonAsync($"/api/users/{user.Id}/reset-password", new UserResetPasswordRequest(user.Id, newPassword));
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    // login with old password, should not work
    _ = await TryLoginAs(user.Email, originalPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    // login with new password, should work
    _ = await TryLoginAs(user.Email, newPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task root_admin_can_reset_any_user_password_in_any_tenant()
  {
    // Todo: find a solution for the multitenant scope
  }

  [Fact]
  public async Task only_admins_can_reset_admins_password()
  {
    // Todo: is this right?
  }

  [Fact]
  public async Task admin_can_change_his_own_password()
  {
    var _ = await TryLoginAs(RootAdminEmail, RootAdminPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var newPassword = RootAdminPassword + "1";
    _ = await RootAdmin_PutAsJsonAsync("/api/personal/change-password", new ChangePasswordRequest
    {
      Password = RootAdminPassword,
      NewPassword = newPassword,
      ConfirmNewPassword = newPassword
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(RootAdminEmail, RootAdminPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    _ = await TryLoginAs(RootAdminEmail, newPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    // Update new password for subsequent tests
    RootAdminPassword = newPassword;
  }

  [Fact]
  public async Task user_can_change_his_password()
  {
    var originalPassword = "Ran60m@pass";
    var username = Guid.NewGuid().ToString();
    var newUser = new CreateUserRequest()
    {
      Email = $"{username}@email.com",
      Password = originalPassword,
      ConfirmPassword = originalPassword,
      FirstName = username,
      LastName = username,
      UserName = username,
    };
    var _ = await RootAdmin_PostAsJsonAsync("/api/users", newUser);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await _.Content.ReadFromJsonAsync<CreateUserResponseDto>();

    _ = await TryLoginAs(user.Email, originalPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var loginHeaders = await LoginAs(user.Email, originalPassword, null, "root");

    var newPassword = originalPassword + "1";
    _ = await PutAsJsonAsync("/api/personal/change-password", new ChangePasswordRequest
    {
      Password = originalPassword,
      NewPassword = newPassword,
      ConfirmNewPassword = newPassword
    }, loginHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(user.Email, originalPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    _ = await TryLoginAs(user.Email, newPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task user_can_reset_his_password()
  {
    var originalPassword = "Ran60m@pass";
    var username = Guid.NewGuid().ToString();
    var newUser = new CreateUserRequest()
    {
      Email = $"{username}@email.com",
      Password = originalPassword,
      ConfirmPassword = originalPassword,
      FirstName = username,
      LastName = username,
      UserName = username,
    };
    var _ = await RootAdmin_PostAsJsonAsync("/api/users", newUser);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var user = await _.Content.ReadFromJsonAsync<CreateUserResponseDto>();

    _ = await TryLoginAs(user.Email, originalPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var mailReceivedTask = new TaskCompletionSource<SmtpMessage>();
    MailReceivedTask = mailReceivedTask;

    // forgot password, should give user the reset password token by email
    var headers = new Dictionary<string, string> { ["tenant"] = "root" };
    _ = await PostAsJsonAsync("/api/users/forgot-password", new ForgotPasswordRequest
    {
      Email = user.Email
    }, headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    // wait for the forgot password email to be received and extract the token from it
    var message = await mailReceivedTask.Task;

    message.Should().NotBeNull();
    message.Subject.Should().Be("Reset Password");
    message.MessageParts.Should().Contain(a => a.BodyData.Contains("Your Password Reset Token is"));
    var messageBody = message.MessageParts[0].BodyData;
    var tokenRegex = new Regex("\'.*\'");
    var tokenMatch = tokenRegex.Match(messageBody);
    tokenMatch.Success.Should().BeTrue();
    var token = tokenMatch.Value.Replace("\'", string.Empty);

    // user can use this token to reset his password
    var newPassword = originalPassword + "1";
    _ = await PostAsJsonAsync("/api/users/reset-password", new ResetPasswordRequest
    {
      Email = user.Email,
      Password = newPassword,
      Token = token
    }, headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(user.Email, newPassword, "root");
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task can_login_as_tenant_admin_and_do_admin_stuff_on_that_tenant()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";
    var username = $"{tenantId}.admin";
    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);

    _ = await RootAdmin_PostAsJsonAsync("/api/support/remote-admin-login", new RemoteAdminLoginRequest
    {
      TenantId = tenantResponse.Id,
      UserName = username,
      Subscription = SubscriptionType.Standard
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tokenResponse = await _.Content.ReadFromJsonAsync<TokenResponse>();
    tokenResponse.Should().NotBeNull();

    // do admin stuff
    var headers = new Dictionary<string, string> { { "tenant", tenantId } };
    headers.Add("Authorization", $"Bearer {tokenResponse.Token}");
    headers.Add(MultitenancyConstants.SubscriptionTypeHeaderName, SubscriptionType.Standard);

    _ = await GetAsync("api/users", headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var users = await _.Content.ReadFromJsonAsync<List<UserDetailsDto>>();
    users.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task root_tenant_admin_can_reset_any_other_tenant_user_password()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";
    var username = $"{tenantId}.admin";
    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);

    _ = await RootAdmin_PostAsJsonAsync("/api/support/reset-other-user-password",
      new ResetRemoteUserPasswordRequest(tenantResponse.Id, username, SubscriptionType.Standard));

    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var newPassword = await _.Content.ReadAsStringAsync();
    newPassword.Should().NotBeNull();

    _ = await TryLoginAs(adminEmail, newPassword, tenantId);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }
}