using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Infrastructure.Middleware;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Subscriptions;

public class SubscriptionTests : TestFixture
{
  public SubscriptionTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task admin_can_create_new_tenant_and_its_admin_can_login()
  {
    HostFixture.SYSTEM_TIME.DaysOffset = 10;

    var tenantId = Guid.NewGuid().ToString();
    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);


    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);

    _ = await PostAsJsonAsync("/api/tokens",
      new TokenRequest(tenant.AdminEmail, TestConstants.DefaultTenantAdminPassword),
      new Dictionary<string, string> { { "tenant", tenantId } });

    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenant_admin_token = await _.Content.ReadFromJsonAsync<TokenResponse>();
    tenant_admin_token.Token.Should().NotBeEmpty();
  }

  [Fact]
  public async Task expired_subscription_cannot_perform_any_operations_unless_renewed()
  {
    var tenantId = Guid.NewGuid().ToString();

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);

    _ = await PostAsJsonAsync("/api/tokens",
      new TokenRequest(tenant.AdminEmail, "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", tenantId } });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tokenResult = await _.Content.ReadFromJsonAsync<TokenResponse>();

    HostFixture.SYSTEM_TIME.DaysOffset = 100;

    _ = await PostAsJsonAsync("/api/v1/products/search",
      new SearchProductsRequest(),
      new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } });

    _.StatusCode.Should().NotBe(HttpStatusCode.OK);
    _.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    var errorResult = await _.Content.ReadFromJsonAsync<ErrorResult>();
    errorResult.Exception.Should().Contain("Subscription expired");

    _ = await RootAdmin_GetAsync($"/api/tenants/{tenantId}");
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantInfo = await _.Content.ReadFromJsonAsync<TenantDto>();
    tenantInfo.Should().NotBeNull();
    tenantInfo.ProdSubscriptionId.Should().NotBeEmpty();

    // response = await RootAdmin_PostAsJsonAsync("/api/tenants/renew", new RenewSubscriptionRequest
    // {
    //   TenantId = tenantId,
    //   SubscriptionId = tenantInfo.ProdSubscriptionId!.Value
    // });
    // response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task demo_operations_are_reset_after_24hours()
  {
  }

  /*
   *
   */
}