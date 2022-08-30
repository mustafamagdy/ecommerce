using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Shared.Multitenancy;
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);
  }

  [Fact]
  public async Task cannot_create_duplicate_tenants()
  {
    var tenantId = Guid.NewGuid().ToString();
    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = $"admin@{tenantId}.com",
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var tenantResponse = await _.Content.ReadFromJsonAsync<BasicTenantInfoDto>();
    tenantResponse.Should().NotBeNull();
    tenantResponse.Id.Should().Be(tenantId);

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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/deactivate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId);
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/deactivate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId);
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    _ = await RootAdmin_PostAsJsonAsync($"/api/tenants/{tenantId}/activate", null);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId);
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId);
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId);
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId);
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
      ProdPackageId = _packages.First().Id,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, MultitenancyConstants.DefaultPassword, null, tenantId);
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
  public async Task different_subscription_has_different_data_for_same_tenant()
  {
    string tenantId = PrepareNewTenant(out string adminEmail, out var tenant);

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId, null, SubscriptionType.Standard);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail, MultitenancyConstants.DefaultPassword, tenantId, null, SubscriptionType.Demo);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantId2 = PrepareNewTenant(out string adminEmail2, out var tenant2, hasDemo: false);

    _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant2);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail2, MultitenancyConstants.DefaultPassword, tenantId2, null, SubscriptionType.Standard);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await TryLoginAs(adminEmail2, MultitenancyConstants.DefaultPassword, tenantId2, null, SubscriptionType.Demo);
    _.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  private string PrepareNewTenant(out string adminEmail, out CreateTenantRequest tenant, bool hasDemo = true)
  {
    var tenantId = Guid.NewGuid().ToString();
    adminEmail = $"admin@{tenantId}.com";
    var username = $"{tenantId}.admin";
    tenant = new CreateTenantRequest
    {
      Id = tenantId,
      ProdPackageId = _packages.First().Id,
      DemoPackageId = hasDemo ? _packages.First().Id : null,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
    };
    return tenantId;
  }
}