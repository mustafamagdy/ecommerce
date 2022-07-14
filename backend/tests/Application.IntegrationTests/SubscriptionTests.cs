using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Multitenancy;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Application.IntegrationTests;

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
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } });

    response.EnsureSuccessStatusCode();

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    response = await PostAsJsonAsync("/api/tenants", tenant, new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } });
    response.EnsureSuccessStatusCode();

    var newTenantId = await response.Content.ReadAsStringAsync();
    newTenantId.Should().BeEquivalentTo(tenant.Id);

    response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest(tenant.AdminEmail, "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", tenantId } });
    response.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task expired_subscription_cannot_perform_any_operations()
  {
    var tenantId = Guid.NewGuid().ToString();
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } });

    response.EnsureSuccessStatusCode();

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    response = await PostAsJsonAsync("/api/tenants", tenant, new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } });
    response.EnsureSuccessStatusCode();

    var newTenantId = await response.Content.ReadAsStringAsync();
    newTenantId.Should().BeEquivalentTo(tenant.Id);

    response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest(tenant.AdminEmail, "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", tenantId } });
    response.EnsureSuccessStatusCode();
    tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    HostFixture.SYSTEM_TIME.DaysOffset = 100;

    response = await PostAsJsonAsync("/api/v1/products/search",
      new SearchProductsRequest(),
      new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } }
    );
    response.EnsureSuccessStatusCode();
  }
}