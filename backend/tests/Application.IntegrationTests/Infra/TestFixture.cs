using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

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

  public Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _client.DefaultRequestHeaders.Add(key, val);
    }

    return _client.PostAsJsonAsync(requestUri, value, cancellationToken);
  }
}