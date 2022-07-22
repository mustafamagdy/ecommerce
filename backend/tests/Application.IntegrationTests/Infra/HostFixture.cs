using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using netDumbster.smtp;
using Xunit;

namespace Application.IntegrationTests.Infra;

public class HostFixture : IAsyncLifetime
{
  private WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();
  public static readonly List<string> DATABASES = new();
  private IDisposable _memoryConfigs;
  private readonly string _cnStringTemplate = "Data Source=localhost;Initial Catalog={0};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true";

  private SimpleSmtpServer _smtpServer;

  public HttpClient CreateClient() => _factory.CreateClient();
  public event EventHandler<MessageReceivedArgs>? MessageReceived = default;

  public Task InitializeAsync()
  {
    var db_name = $"main_{Guid.NewGuid()}";
    DATABASES.Add(db_name);

    _memoryConfigs = Program.OverrideConfig(new Dictionary<string, string>
    {
      ["DatabaseSettings:ConnectionString"] = string.Format(_cnStringTemplate, db_name),
      ["HangfireSettings:Storage:ConnectionString"] = string.Format($"{_cnStringTemplate};Allow User Variables=true", db_name)
    });

    _factory = new TestWebApplicationFactory();

    // Port 5221 is configured in mail app settings json file
    _smtpServer = SimpleSmtpServer.Start(5221);
    _smtpServer.MessageReceived += SmtpServerOnMessageReceived;

    return Task.CompletedTask;
  }

  private void SmtpServerOnMessageReceived(object? sender, MessageReceivedArgs e)
  {
    if (MessageReceived != null)
    {
      MessageReceived.Invoke(sender, e);
    }
  }

  public async Task DisposeAsync()
  {
    await _factory.DisposeAsync();
    _memoryConfigs.Dispose();
    _smtpServer.Dispose();

    var cs = string.Format(_cnStringTemplate, "sys");
    var cmdStr = "drop schema if exists `{0}`;";
    await using var cn = new MySqlConnection(cs);
    await cn.OpenAsync();

    foreach (var db in DATABASES)
    {
      await using var cmd = cn.CreateCommand();
      cmd.CommandText = string.Format(cmdStr, db);
      await cmd.ExecuteNonQueryAsync();
    }

    cn.Clone();
    await cn.DisposeAsync();
  }
}