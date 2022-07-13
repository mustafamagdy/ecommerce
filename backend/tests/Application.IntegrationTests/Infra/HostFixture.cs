using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class HostFixture : IDisposable
{
  private readonly WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();

  public HostFixture()
  {
    _factory = new TestWebApplicationFactory();
  }

  public HttpClient CreateClient() => _factory.CreateClient();

  public void Dispose()
  {
    _factory.Dispose();
  }
}