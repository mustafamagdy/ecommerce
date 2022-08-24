using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
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
    basicTenantInfo.ProdSubscription.Should().NotBeNull();
    basicTenantInfo.ProdSubscription.ExpiryDate.Should().BeAfter(today);
  }

  [Fact]
  public async Task can_get_tenant_subscription_history()
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
    basicTenantInfo.ProdSubscription.Should().NotBeNull();
    basicTenantInfo.ProdSubscription.ExpiryDate.Should().BeAfter(today);

    _ = await PostAsJsonAsync("/api/v1/my/subscription", new MyTenantSubscriptionSearchRequest
      {
        SubscriptionId = basicTenantInfo.ProdSubscription.Id
      },
      tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var subscriptionInfo = await _.Content.ReadFromJsonAsync<ProdTenantSubscriptionDto>();
    subscriptionInfo.Should().NotBeNull();
    subscriptionInfo.ExpiryDate.Should().BeAfter(today);
    subscriptionInfo.History.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task can_get_tenant_subscription_with_payments()
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
    basicTenantInfo.ProdSubscription.Should().NotBeNull();
    basicTenantInfo.ProdSubscription.ExpiryDate.Should().BeAfter(today);
    basicTenantInfo.TotalDue.Should().BeGreaterThan(0);
    basicTenantInfo.TotalPaid.Should().Be(0);
    basicTenantInfo.Active.Should().Be(true);

    _ = await PostAsJsonAsync("/api/v1/my/payments", new MyTenantSubscriptionAndPaymentsSearchRequest
      {
        SubscriptionId = basicTenantInfo.ProdSubscription.Id
      },
      tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var subscriptionInfo = await _.Content.ReadFromJsonAsync<ProdTenantSubscriptionWithPaymentDto>();
    subscriptionInfo.Should().NotBeNull();
    subscriptionInfo.ExpiryDate.Should().BeAfter(today);
    subscriptionInfo.History.Should().NotBeNullOrEmpty();
    subscriptionInfo.Payments.Should().BeEmpty();

    _ = await RootAdmin_PostAsJsonAsync("/api/tenants/pay", new PayForSubscriptionRequest
    {
      Amount = 100,
      PaymentMethodId = null,
      SubscriptionId = subscriptionInfo.SubscriptionId
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await GetAsync("/api/v1/my", tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    basicTenantInfo = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    basicTenantInfo.TotalDue.Should().NotBe(0);
    basicTenantInfo.TotalPaid.Should().BeGreaterThan(0);

    _ = await PostAsJsonAsync("/api/v1/my/payments", new MyTenantSubscriptionAndPaymentsSearchRequest
      {
        SubscriptionId = basicTenantInfo.ProdSubscription.Id
      },
      tenantAdminLoginHeaders, CancellationToken.None);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    subscriptionInfo = await _.Content.ReadFromJsonAsync<ProdTenantSubscriptionWithPaymentDto>();
    subscriptionInfo.Should().NotBeNull();
    subscriptionInfo.ExpiryDate.Should().BeAfter(today);
    subscriptionInfo.History.Should().NotBeEmpty();
    subscriptionInfo.Payments.Should().NotBeEmpty();
  }

  [Fact]
  public async Task can_login_as_tenant_admin_and_do_admin_stuff_on_that_tenant()
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

    _ = await RootAdmin_PostAsJsonAsync("/api/tenants/remote-admin-login", new RemoteAdminLoginRequest
    {
      TenantId = responseTenantId
    });
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tokenResponse = await _.Content.ReadFromJsonAsync<TokenResponse>();
    tokenResponse.Should().NotBeNull();

    // do admin stuff
    var headers = new Dictionary<string, string> { { "tenant", tenantId } };
    headers.Add("Authorization", $"Bearer {tokenResponse.Token}");
    _ = await GetAsync("api/users", headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var users = await _.Content.ReadFromJsonAsync<List<UserDetailsDto>>();
    users.Should().NotBeNullOrEmpty();
  }
}