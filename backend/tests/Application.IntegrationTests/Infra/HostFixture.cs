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

  private readonly string dbProvider = "postgresql"; // postgresql // mysql
  private string _cnStringTemplate = "";

  private SimpleSmtpServer _smtpServer;

  public HttpClient CreateClient() => _factory.CreateClient();

  public event EventHandler<MessageReceivedArgs>? MessageReceived = default;

  public async Task InitializeAsync()
  {
    _cnStringTemplate = dbProvider switch
    {
      "postgresql" => "Server=127.0.0.1;Port={0};Database={1};Uid=postgres;Pwd=DeV12345",
      "mysql" => "Data Source=127.0.0.1;Port={0};Initial Catalog={1};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true;Allow User Variables=true;",
      _ => throw new ArgumentOutOfRangeException()
    };

    _dbContainer = BuildContainer();
    await _dbContainer.StartAsync();

    var db_name = $"main_{Guid.NewGuid()}";
    var connectionString = string.Format(_cnStringTemplate, _dbPort, db_name);
    var tenantDbConnectionStringTemplate = dbProvider switch
    {
      "postgresql" => $"Server=127.0.0.1;Port={_dbPort};Database={{0}};Uid=postgres;Pwd=DeV12345",
      "mysql" => $"Data Source=127.0.0.1;Port={_dbPort};Initial Catalog={{0}};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true;Allow User Variables=true;",
      _ => throw new ArgumentOutOfRangeException()
    };
    var mailPort = GetFreeTcpPort();

    _memoryConfigs = Program.OverrideConfig(new Dictionary<string, string>
    {
      ["DatabaseSettings:DBProvider"] = dbProvider,
      ["DatabaseSettings:ConnectionString"] = connectionString,
      ["DatabaseSettings:ConnectionStringTemplate"] = tenantDbConnectionStringTemplate,
      // ["HangfireSettings:Storage:ConnectionString"] = connectionString,
      // ["HangfireSettings:Storage:StorageProvider"] = dbProvider,
      ["HangfireSettings:Storage:ConnectionString"] = "",
      ["HangfireSettings:Storage:StorageProvider"] = "",
      ["MailSettings:Port"] = mailPort.ToString()
    });

    _factory = new TestWebApplicationFactory(_hostPort);

    _smtpServer = SimpleSmtpServer.Start(mailPort);
    _smtpServer.MessageReceived += SmtpServerOnMessageReceived;
  }

  private TestcontainersContainer BuildContainer()
  {
    _dbPort = GetFreeTcpPort();
    var internalPort = dbProvider switch
    {
      "postgresql" => 5432,
      "mysql" => 3306,
      _ => throw new ArgumentOutOfRangeException()
    };

    ITestcontainersBuilder<TestcontainersContainer> builder = new TestcontainersBuilder<TestcontainersContainer>();
    builder = dbProvider switch
    {
      "postgresql" => builder.WithImage("postgres:alpine")
        .WithEnvironment("POSTGRES_PASSWORD", "DeV12345"),

      "mysql" => builder.WithImage("amd64/mysql:8.0-oracle")
        .WithEnvironment("MYSQL_ROOT_PASSWORD", "DeV12345")
        .WithEnvironment("MYSQL_PASSWORD", "DeV12345"),
      _ => throw new ArgumentOutOfRangeException()
    };

    builder = builder.WithPortBinding(_dbPort, internalPort)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(internalPort));

    return builder.Build();
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

    // await _dbContainer.StopAsync();
    _memoryConfigs.Dispose();
    _smtpServer.Dispose();
  }
}