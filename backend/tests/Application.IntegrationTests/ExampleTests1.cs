using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests;

public class ExampleTests1 : TestFixture
{
  public ExampleTests1(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  [Fact]
  public async Task admin_of_root_tenant_can_list_all_products()
  {
    _output.WriteLine("Start testing ..");
    var response = await PostAsJsonAsync("/api/tokens",
      new TokenRequest("admin@root.com", "123Pa$$word!"),
      new Dictionary<string, string> { { "tenant", "root" } });

    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    _output.WriteLine("Token is " + tokenResult.Token);

    response = await PostAsJsonAsync("/api/v1/products/search",
      new SearchProductsRequest(),
      new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResult.Token}" } }
    );

    response.EnsureSuccessStatusCode();

    var productResult = await response.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    productResult.Should().NotBeNull();
    productResult.TotalCount.Should().BeGreaterThan(1);
  }
}