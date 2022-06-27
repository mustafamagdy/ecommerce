using System.Net.Http.Json;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Identity.Tokens;
using FSH.WebApi.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

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
    var token = await client.PostAsJsonAsync("/api/tokens", new TokenRequest("admin@root.com", "123Pa$$word!"), CancellationToken.None);
    var products = await client.PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest());
  }
}