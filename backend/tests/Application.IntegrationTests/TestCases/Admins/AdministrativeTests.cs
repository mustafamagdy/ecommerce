using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Tokens;
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

  /*
   * admin_can_create_user_with_predefined_role_and_that_user_can_login
   * disabled_user_cannot_login
   * users_cannot_create_themselves_when_this_feature_is_disabled
   * admin_can_reset_any_user_password
   * only_admins_can_reset_admins_password
   * admin_can_change_his_own_password
   * user_can_change_his_password
   * user_can_reset_his_password
   * admin_can_create_role_and_add_permissions
   * user_cannot_access_resource_unless_he_has_permission
   *
   */
}