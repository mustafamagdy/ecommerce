using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class TenantManagementTests : TestFixture
{
  public TenantManagementTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   * can_create_new_tenants_when_submit_valid_data
   * cannot_create_duplicate_tenants
   * deactivated_tenants_cannot_login
   * can_activate_deactivate_tenants
   * can_pay_for_tenant_subscription
   * can_get_basic_tenant_info
   * can_get_tenant_subscription_history
   * can_get_tenant_subscription_with_payments
   *
   */
}