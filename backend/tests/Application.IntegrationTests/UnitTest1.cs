using System.Net.Http.Json;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Application.Multitenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Application.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureServices(services =>
    {
    });
    base.ConfigureWebHost(builder);
  }
}

public class HostFixture : IDisposable
{
  private readonly WebApplicationFactory<Program> _factory;
  internal readonly HttpClient Client;

  public HostFixture()
  {
    _factory = new TestWebApplicationFactory();
    Client = _factory.CreateClient();
  }

  public void Dispose()
  {
    _factory.Dispose();
  }
}

public abstract class TestFixture : IDisposable, IClassFixture<HostFixture>
{
  private readonly HostFixture _host;
  protected readonly ITestOutputHelper _output;

  public TestFixture(HostFixture host, ITestOutputHelper output)
  {
    _host = host;
    _output = output;
  }

  public Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string? requestUri, TValue value, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
  {
    _host.Client.DefaultRequestHeaders.Clear();
    foreach ((string? key, string? val) in headers)
    {
      _host.Client.DefaultRequestHeaders.Add(key, val);
    }

    return _host.Client.PostAsJsonAsync(requestUri, value, cancellationToken);
  }

  public void Dispose()
  {
    _output.WriteLine("Disposing ...");
    _host.Dispose();
  }
}

public class ExampleTests1 : TestFixture
{
  public ExampleTests1(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task Test1()
  {
    _output.WriteLine("Start testing ..");
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } });

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    _output.WriteLine("Token is " + tokenResult.Token);

    response = await PostAsJsonAsync("/api/v1/products/search",
      new SearchProductsRequest(),
      new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } });

    if (response.IsSuccessStatusCode)
    {
      var productResult = await response.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    }
  }
}

public class SubscriptionTests : TestFixture
{
  public SubscriptionTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task admin_can_create_new_tenant()
  {
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } });

    response.EnsureSuccessStatusCode();

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    var tenant = new CreateTenantRequest
    {
      Id = "tenant01",
      Email = "email@tenant01.com",
      AdminEmail = "admin@tenant01.com",
      Name = "Tenant 01",
      DatabaseName = "tenant01-db",
    };

    response = await PostAsJsonAsync("/api/tenants", tenant, new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } });
    response.EnsureSuccessStatusCode();

    var tenantId = await response.Content.ReadAsStringAsync();
    tenantId.Should().BeEquivalentTo(tenant.Id);
  }
}