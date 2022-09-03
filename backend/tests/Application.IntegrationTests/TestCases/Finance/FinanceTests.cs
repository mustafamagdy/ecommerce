using Application.IntegrationTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Finance;

// [Collection(nameof(TestConstants.WebHostTests))]
public class FinanceTests : TestFixture
{
  private readonly HostFixture _host;

  public FinanceTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
    _host = host;
  }

  [Fact]
  public Task test_01()
  {
    Output.WriteLine($"Id 1 is {_host.Instance}");
    return Task.Delay(2000);
  }

  [Fact]
  public Task test_02()
  {
    Output.WriteLine($"Id 2 is {_host.Instance}");
    return Task.Delay(1000);
  }
  /*
   *
   */
}