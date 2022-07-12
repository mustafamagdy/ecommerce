using System.Net.Http.Json;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Tokens;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Application.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override IHostBuilder? CreateHostBuilder()
  {
    return base.CreateHostBuilder();
  }
}
public class UnitTest1
{
  public UnitTest1()
  {
  }

  [Fact]
  public async Task Test1()
  {
    var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });
    // var application = new TestWebApplicationFactory();

    var client = application.CreateClient();

    client.DefaultRequestHeaders.Add("tenant", "root");
    var response = await client.PostAsJsonAsync("/api/tokens", new TokenRequest("admin@root.com", "123Pa$$word!"), CancellationToken.None);
    var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
    response = await client.PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest());
    if (response.IsSuccessStatusCode)
    {
      var productResult = await response.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    }
  }
}