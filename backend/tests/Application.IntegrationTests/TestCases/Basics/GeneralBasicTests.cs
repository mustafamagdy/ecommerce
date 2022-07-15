using Application.IntegrationTests.Infra;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Basics;

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
   */
}