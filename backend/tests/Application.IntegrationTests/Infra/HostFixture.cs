using System.Net;
using System.Net.Sockets;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using netDumbster.smtp;
using Xunit;

namespace Application.IntegrationTests.Infra;

public class HostFixture : IAsyncLifetime
{
  private int _dbPort = GetFreeTcpPort();
  private int _hostPort = GetFreeTcpPort();
  private TestcontainersContainer _dbContainer;

  private WebApplicationFactory<Program> _factory;
  public static readonly TestSystemTime SYSTEM_TIME = new();
  private IDisposable _memoryConfigs;
  private readonly string _cnStringTemplate = "Data Source=localhost;Port={0};Initial Catalog={1};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true";

  private SimpleSmtpServer _smtpServer;

  public HttpClient CreateClient() => _factory.CreateClient();
  // public HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri($"http://localhost:{_hostPort}") });

  public event EventHandler<MessageReceivedArgs>? MessageReceived = default;

  public async Task InitializeAsync()
  {
    _dbContainer = BuildContainer();
    await _dbContainer.StartAsync();

    var db_name = $"main_{Guid.NewGuid()}";
    var tenantConnectionStringTemplate = "Data Source=127.0.0.1;Port=" + _dbPort + ";Initial Catalog={0};User Id=root;Password=DeV12345";
    var mailPort = GetFreeTcpPort();

    _memoryConfigs = Program.OverrideConfig(new Dictionary<string, string>
    {
      ["DatabaseSettings:ConnectionString"] = string.Format(_cnStringTemplate, _dbPort, db_name),
      ["DatabaseSettings:ConnectionStringTemplate"] = tenantConnectionStringTemplate,
      ["HangfireSettings:Storage:ConnectionString"] = string.Format($"{_cnStringTemplate};Allow User Variables=true", _dbPort, db_name),
      ["MailSettings:Port"] = mailPort.ToString()
    });

    _factory = new TestWebApplicationFactory(_hostPort);

    _smtpServer = SimpleSmtpServer.Start(mailPort);
    _smtpServer.MessageReceived += SmtpServerOnMessageReceived;
  }

  private TestcontainersContainer BuildContainer()
  {
    _dbPort = GetFreeTcpPort();
    return new TestcontainersBuilder<TestcontainersContainer>()
      .WithImage("amd64/mysql:8.0-oracle")
      .WithEnvironment("MYSQL_ROOT_PASSWORD", "DeV12345")
      .WithEnvironment("MYSQL_PASSWORD", "DeV12345")
      .WithPortBinding(_dbPort, 3306)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
      .Build();
  }

  private static int GetFreeTcpPort()
  {
    var l = new TcpListener(IPAddress.Loopback, 0);
    l.Start();
    int port = ((IPEndPoint)l.LocalEndpoint).Port;
    l.Stop();
    return port;
  }

  private void SmtpServerOnMessageReceived(object? sender, MessageReceivedArgs e)
  {
    MessageReceived?.Invoke(sender, e);
  }

  public async Task DisposeAsync()
  {
    await _factory.DisposeAsync();

    await _dbContainer.StopAsync();
    _memoryConfigs.Dispose();
    _smtpServer.Dispose();
  }
}