using System.Net;
using System.Net.Sockets;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using netDumbster.smtp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Application.IntegrationTests.Infra;

public class HostFixture : IAsyncLifetime
{
  public Guid Instance;
  private readonly int _dbPort = GetFreeTcpPort();
  private readonly int _hostPort = GetFreeTcpPort();
  private readonly int _mailPort = GetFreeTcpPort();
  private TestcontainersContainer _dbContainer;
  private WebApplicationFactory<Program> _factory;
  private readonly IMessageSink _sink;
  private string _cnStringTemplate = "";
  private SimpleSmtpServer _smtpServer;
  public event EventHandler<MessageReceivedArgs>? MessageReceived = default;
  public static readonly TestSystemTime SYSTEM_TIME = new();

  public HostFixture(IMessageSink sink)
  {
    Instance = Guid.NewGuid();

    this._sink = sink;
    sink.OnMessage(new DiagnosticMessage("Host fixture is being created"));
    Environment.SetEnvironmentVariable("db-provider", "postgresql"); // postgresql // mysql
  }

  private string DbProvider => Environment.GetEnvironmentVariable("db-provider")!;

  public async Task InitializeAsync()
  {
    _cnStringTemplate = DbProvider switch
    {
      "postgresql" => "Server=127.0.0.1;Port={0};Database={1};Uid=postgres;Pwd=DeV12345",
      "mysql" => "Data Source=127.0.0.1;Port={0};Initial Catalog={1};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true;Allow User Variables=true;",
      _ => throw new ArgumentOutOfRangeException()
    };

    _dbContainer = BuildContainer();
    await _dbContainer.StartAsync();

    var db_name = $"main_{Guid.NewGuid()}";
    var connectionString = string.Format(_cnStringTemplate, _dbPort, db_name);
    var tenantDbConnectionStringTemplate = DbProvider switch
    {
      "postgresql" => $"Server=127.0.0.1;Port={_dbPort};Database={{0}};Uid=postgres;Pwd=DeV12345",
      "mysql" => $"Data Source=127.0.0.1;Port={_dbPort};Initial Catalog={{0}};User Id=root;Password=DeV12345;SSL Mode=None;AllowPublicKeyRetrieval=true;Allow User Variables=true;",
      _ => throw new ArgumentOutOfRangeException()
    };

    var testConfigs = new Dictionary<string, string>
    {
      ["DatabaseSettings:DBProvider"] = DbProvider,
      ["DatabaseSettings:ConnectionString"] = connectionString,
      ["DatabaseSettings:ConnectionStringTemplate"] = tenantDbConnectionStringTemplate,
      ["HangfireSettings:Storage:ConnectionString"] = "",
      ["HangfireSettings:Storage:StorageProvider"] = "",
      ["MailSettings:Port"] = _mailPort.ToString()
    };

    foreach (var config in testConfigs)
    {
      Environment.SetEnvironmentVariable(config.Key, config.Value);
    }

    _factory = new TestWebApplicationFactory(_hostPort);

    _smtpServer = SimpleSmtpServer.Start(_mailPort);
    _smtpServer.MessageReceived += SmtpServerOnMessageReceived;
  }

  internal HttpClient CreateClient() => _factory.CreateClient();

  private TestcontainersContainer BuildContainer()
  {
    var internalPort = DbProvider switch
    {
      "postgresql" => 5432,
      "mysql" => 3306,
      _ => throw new ArgumentOutOfRangeException()
    };

    ITestcontainersBuilder<TestcontainersContainer> builder = new TestcontainersBuilder<TestcontainersContainer>();
    builder = DbProvider switch
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
    await _dbContainer.StopAsync();
    _smtpServer.Dispose();
  }
}