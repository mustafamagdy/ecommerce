using System.Net.Http.Json;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Identity.Tokens;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace Application.IntegrationTests;

public class UnitTest1
{
  public UnitTest1()
  {
  }

  [Fact]
  public async Task Test1()
  {
    var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => { });

    var client = application.CreateClient();

    client.DefaultRequestHeaders.Add("tenant", "root");
    var response = await client.PostAsJsonAsync("/api/tokens", new TokenRequest("admin@root.com", "123Pa$$word!"), CancellationToken.None);
    var content = await response.Content.ReadAsStringAsync();
    var tokenResult = JsonConvert.DeserializeObject<TokenResponse>(content);

    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResult.Token}");
    response = await client.PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest());
    if (response.IsSuccessStatusCode)
    {
      content = await response.Content.ReadAsStringAsync();
      var productResult = JsonConvert.DeserializeObject<PaginationResponse<ProductDto>>(content);
    }
  }
}