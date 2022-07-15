using Microsoft.AspNetCore.Mvc.Testing;
using MySqlConnector;

namespace Application.IntegrationTests.Infra;

public class HostFixture : IDisposable
{
  private readonly WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();
  public static readonly List<string> DATABASES = new();
  private readonly IDisposable _memoryConfigs;
  private readonly string _cnStringTemplate;

  public HostFixture()
  {
    var db_name = $"main_{Guid.NewGuid()}";
    DATABASES.Add(db_name);

    _cnStringTemplate = "Data Source=127.0.0.1;Initial Catalog={0};User Id=root;Password=DeV12345";
    _memoryConfigs = Program.OverrideConfig(new Dictionary<string, string>
    {
      ["DatabaseSettings:ConnectionString"] = string.Format(_cnStringTemplate, db_name),
      ["HangfireSettings:Storage:ConnectionString"] = string.Format($"{_cnStringTemplate};Allow User Variables=true", db_name)
    });
    var x = Program.InMemoryConfig;

    _factory = new TestWebApplicationFactory();
  }

  public HttpClient CreateClient() => _factory.CreateClient();

  public void Dispose()
  {
    _factory.Dispose();
    _memoryConfigs.Dispose();

    var cs = string.Format(_cnStringTemplate, "sys");
    var cmdStr = "drop schema if exists `{0}`;";
    using var cn = new MySqlConnection(cs);
    cn.Open();

    foreach (var db in DATABASES)
    {
      using var cmd = cn.CreateCommand();
      cmd.CommandText = string.Format(cmdStr, db);
      cmd.ExecuteNonQuery();
    }

    cn.Clone();
    cn.Dispose();
  }
}