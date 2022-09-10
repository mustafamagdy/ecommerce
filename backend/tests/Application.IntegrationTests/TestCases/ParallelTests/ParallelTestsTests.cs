using Application.IntegrationTests.Infra;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.ParallelTests;

public class ParallelTest1 : TestFixture
{
  private readonly HostFixture _host;

  public ParallelTest1(HostFixture host, ITestOutputHelper output)
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
}

public class ParallelTest2 : TestFixture
{
  private readonly HostFixture _host;

  public ParallelTest2(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
    _host = host;
  }

  [Fact]
  public Task test_02()
  {
    Output.WriteLine($"Id 2 is {_host.Instance}");
    return Task.Delay(1000);
  }
}