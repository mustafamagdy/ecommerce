using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Catalog.Services;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Catalog;

// [Collection(nameof(TestConstants.WebHostTests))]
public class ProductTests : TestFixture
{
  public ProductTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  // products

  [Fact]
  public async Task can_list_product()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
  }

  [Fact]
  public async Task can_update_product_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/products", new CreateProductRequest
    {
      Name = "new product",
      Description = "",
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var productId = await _.Content.ReadFromJsonAsync<Guid>();

    var updateProduct = new UpdateProductRequest
    {
      Id = productId,
      Name = "new product2",
      Description = ""
    };
    _ = await PutAsJsonAsync($"/api/v1/products/{productId}", updateProduct, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.FirstOrDefault(a => a.Id == productId);
    product.Should().NotBeNull();
    product.Name.Should().Be(updateProduct.Name);
  }

  [Fact]
  public async Task can_delete_product_when_product_not_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();

    var _ = await PostAsJsonAsync("/api/v1/products", new CreateProductRequest
    {
      Name = "new product",
      Description = "",
      Rate = 10,
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var productId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await DeleteAsJsonAsync($"/api/v1/products/{productId}", new DeleteProductRequest(productId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_delete_product_when_product_is_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.First();

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogs = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogs.Should().NotBeNull();
    catalogs.Data.Should().NotBeEmpty();
    var catalogItem = catalogs.Data.FirstOrDefault(a => a.ProductName == product.Name);
    catalogItem.Should().NotBeNull();

    _ = await DeleteAsJsonAsync($"/api/v1/products/{product.Id}", new DeleteProductRequest(product.Id), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

}