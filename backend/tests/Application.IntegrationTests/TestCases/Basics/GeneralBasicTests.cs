using Application.IntegrationTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Basics;

// [Collection(nameof(TestConstants.WebHostTests))]
public class GeneralBasicTests : TestFixture
{
  public GeneralBasicTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  /*
   * can_query_with_advanced_search
   * can_query_with_advanced_filters
   * can_query_with_pagination
   * server_respond_with_404_for_not_found
   * server_respond_with_403_for_unauthorized_access
   * server_respond_with_400_for_invalid_or_missing_inputs
   */
}