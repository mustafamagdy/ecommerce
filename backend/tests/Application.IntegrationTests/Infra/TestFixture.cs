using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy;
using netDumbster.smtp;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Infra;

[Collection(nameof(TestConstants.WebHostTests))]
// public abstract class TestFixture : IClassFixture<HostFixture>
public abstract class TestFixture
{
  private readonly HostFixture _host;
  private readonly HttpClient _client;
  protected readonly ITestOutputHelper _output;
  protected static string RootAdminPassword = "123Pa$$word!";
  protected static string RootAdminEmail = "admin@root.com";

  protected TestFixture(HostFixture host, ITestOutputHelper output)
  {
    _host = host;
    _output = output;

    _client = _host.CreateClient();
    _output.WriteLine("New http client created");
    _host.MessageReceived += HostOnMessageReceived;
  }

  protected TaskCompletionSource<SmtpMessage>? MailReceivedTask;

  private void HostOnMessageReceived(object? sender, MessageReceivedArgs e)
  {
    MailReceivedTask?.SetResult(e.Message);
  }

  public Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _client.DefaultRequestHeaders.Add(key, val);
    }

    return _client.PostAsJsonAsync(requestUri, value, cancellationToken);
  }

  public Task<HttpResponseMessage> RootAdmin_PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
    => RootAdmin_PostAsJsonAsync(requestUri, (object)value, headers, cancellationToken);

  public async Task<HttpResponseMessage> RootAdmin_PostAsJsonAsync(string? requestUri, object value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await PostAsJsonAsync(requestUri, value, headers, cancellationToken);
  }

  public Task<HttpResponseMessage> PutAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _client.DefaultRequestHeaders.Add(key, val);
    }

    return _client.PutAsJsonAsync(requestUri, value, cancellationToken);
  }

  public async Task<HttpResponseMessage> RootAdmin_PutAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await PutAsJsonAsync(requestUri, value, headers, cancellationToken);
  }

  public Task<HttpResponseMessage> GetAsync(string? requestUri, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _client.DefaultRequestHeaders.Add(key, val);
    }

    return _client.GetAsync(requestUri, cancellationToken);
  }

  public async Task<HttpResponseMessage> RootAdmin_GetAsync(string? requestUri, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    headers = await LoginAsRootAdmin(headers, cancellationToken);
    return await GetAsync(requestUri, headers, cancellationToken);
  }

  protected async Task<Dictionary<string, string>> CreateTenantAndLogin()
  {
    var tenantId = Guid.NewGuid().ToString();
    string adminEmail = $"admin@{tenantId}.com";

    var tenant = new CreateTenantRequest
    {
      Id = tenantId,
      Email = $"email@{tenantId}.com",
      AdminEmail = adminEmail,
      Name = $"Tenant {tenantId}",
      DatabaseName = $"{tenantId}-db",
    };

    var _ = await RootAdmin_PostAsJsonAsync("/api/tenants", tenant);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var tenantAdminLoginHeaders = await LoginAs(adminEmail, TestConstants.DefaultTenantAdminPassword, null, tenantId, CancellationToken.None);
    tenantAdminLoginHeaders.Should().NotBeNullOrEmpty();
    return tenantAdminLoginHeaders;
  }

  protected async Task<List<BasicUserDataDto>> GetUserList(Dictionary<string, string> headers)
  {
    var _ = await GetAsync("/api/users/basic", headers);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    return await _.Content.ReadFromJsonAsync<List<BasicUserDataDto>>();
  }

  public Task<HttpResponseMessage> TryLoginAs(string username, string password, string? tenant, CancellationToken cancellationToken)
  {
    var tenantHeader = tenant != null ? new Dictionary<string, string> { { "tenant", tenant } } : new Dictionary<string, string> { { "tenant", "root" } };
    return PostAsJsonAsync("/api/tokens", new TokenRequest(username, password), tenantHeader, cancellationToken);
  }

  public async Task<Dictionary<string, string>> LoginAs(string username, string password, Dictionary<string, string>? headers, string? tenant, CancellationToken cancellationToken)
  {
    var response = await TryLoginAs(username, password, tenant, cancellationToken);
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    _output.WriteLine("Token is " + tokenResult.Token);

    headers = headers == null ? new Dictionary<string, string>() : headers;

    headers.Add("Authorization", $"Bearer {tokenResult.Token}");
    return headers;
  }

  private Task<Dictionary<string, string>> LoginAsRootAdmin(Dictionary<string, string> headers, CancellationToken cancellationToken)
  {
    return LoginAs(RootAdminEmail, RootAdminPassword, headers, "root", cancellationToken);
  }
}