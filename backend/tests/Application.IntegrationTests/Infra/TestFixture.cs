using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FSH.WebApi.Application.Identity.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.Infra;

[Collection(nameof(TestConstants.WebHostTests))]
public abstract class TestFixture
{
  private readonly HostFixture _host;
  private readonly HttpClient _client;
  protected readonly ITestOutputHelper _output;

  public TestFixture(HostFixture host, ITestOutputHelper output)
  {
    _host = host;
    _output = output;

    _client = _host.CreateClient();
    _output.WriteLine("New http client created");
  }

  public void RemoveThisDbAfterFinish(string db)
  {
    var dbName = $"{TestConstants.TestEnvironmentName}-{db}-db";
    HostFixture.DATABASES.Add(dbName);
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

  public async Task<HttpResponseMessage> RootAdmin_PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers = default!, CancellationToken cancellationToken = default)
  {
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } }, cancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    _output.WriteLine("Token is " + tokenResult.Token);

    headers = headers == null ? new Dictionary<string, string>() : headers;

    headers.Add("Authorization", $"Bearer {tokenResult.Token}");
    return await PostAsJsonAsync(requestUri, value, headers, cancellationToken);
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
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } }, cancellationToken);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
    _output.WriteLine("Token is " + tokenResult.Token);

    headers = headers == null ? new Dictionary<string, string>() : headers;
    headers.Add("Authorization", $"Bearer {tokenResult.Token}");
    return await GetAsync(requestUri, headers, cancellationToken);
  }
}