using Microsoft.AspNetCore.Mvc.Testing;

namespace Application.IntegrationTests.Infra;

public class HostFixture : IDisposable
{
  private readonly WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();

  public HostFixture()
  {
    var db_name = $"main_{Guid.NewGuid()}";
    using var _ = Program.OverrideConfig(new Dictionary<string, string>
    {
      ["DatabaseSettings:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={db_name};User Id=root;Password=DeV12345",
      ["HangfireSettings:Storage:ConnectionString"] = $"Data Source=127.0.0.1;Initial Catalog={db_name};User Id=root;Password=DeV12345;Allow User Variables=true"
    });

    _factory = new TestWebApplicationFactory();
  }

  public HttpClient CreateClient() => _factory.CreateClient();

  public void Dispose()
  {
    _factory.Dispose();
  }
}