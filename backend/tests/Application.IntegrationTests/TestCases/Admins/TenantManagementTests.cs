using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Multitenancy;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class TenantManagementTests : TestFixture
{
  public TenantManagementTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task can_create_new_tenants_when_submit_valid_data()
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
    var responseTenantId = await _.Content.ReadAsStringAsync();
    responseTenantId.Should().Be(tenantId);
  }

  [Fact]
  public async Task cannot_create_duplicate_tenants()
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
    var responseTenantId = await _.Content.ReadAsStringAsync();
    responseTenantId.Should().Be(tenantId);

    _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task deactivated_tenants_cannot_login()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, tenantId, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/deactivate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, tenantId, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task can_activate_deactivate_tenants()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, tenantId, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/deactivate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, tenantId, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/activate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, tenantId, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task can_pay_for_tenant_subscription()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, null, tenantId, CancellationToken.None);
    tenantAdminLoginHeaders.Should().NotBeNullOrEmpty();

    _ = await GetAsync("/api/v1/my", tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var basicTenantInfo = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    basicTenantInfo.Should().NotBeNull();
    basicTenantInfo.ProdSubscription.Should().NotBeNull();
  }

  [Fact]
  public async Task can_get_basic_tenant_info()
  {
    var today = HostFixture.SYSTEM_TIME.Now;
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, null, tenantId, CancellationToken.None);
    tenantAdminLoginHeaders.Should().NotBeNullOrEmpty();

    _ = await GetAsync("/api/v1/my", tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var basicTenantInfo = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    basicTenantInfo.Should().NotBeNull();
    basicTenantInfo.ProdSubscription
      .Should().NotBeNull()
      .And.Subject.As<BasicSubscriptionInfoDto>()
      .ExpiryDate.Should().BeAfter(today);
  }

  [Fact]
  public async Task can_get_tenant_subscription_history()
  {
  }

  [Fact]
  public async Task can_get_tenant_subscription_with_payments()
  {
  }
}