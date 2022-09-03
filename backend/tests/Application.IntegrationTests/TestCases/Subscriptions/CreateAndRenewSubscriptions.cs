using Application.IntegrationTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Subscriptions;

// [Collection(nameof(TestConstants.WebHostTests))]
public class CreateAndRenewSubscriptions : TestFixture
{
  public CreateAndRenewSubscriptions(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   *
   */
}