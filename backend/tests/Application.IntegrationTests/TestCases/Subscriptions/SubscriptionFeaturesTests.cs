using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Subscriptions;

public class SubscriptionFeaturesTests : TestFixture
{
  public SubscriptionFeaturesTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   * user_can_login_as_default_feature_for_all_packages
   * user_can_access_package_neutral_resources
   * user_cannot_access_feature_unless_he_subscribe_for_it
   *
   */
}