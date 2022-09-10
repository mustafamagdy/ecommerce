using Application.IntegrationTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Finance;

public class FinanceTests : TestFixture
{
  private readonly HostFixture _host;

  public FinanceTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
    _host = host;
  }

  /*
   *
   */
}