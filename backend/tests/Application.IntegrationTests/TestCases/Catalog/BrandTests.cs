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

public class BrandTests : TestFixture
{
  public BrandTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  // brands

  [Fact]
  public async Task can_create_brand_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/brands/search", new SearchBrandsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var brands = await _.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();
    brands.Should().NotBeNull();
    brands.Data.Should().NotBeNullOrEmpty();
    var itemCount = brands.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    var newBrand = new Brand(Guid.NewGuid().ToString(), "");
    _ = await PostAsJsonAsync("/api/v1/brands", new CreateBrandRequest
    {
      Name = newBrand.Name,
      Description = newBrand.Description
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/brands/search", new SearchBrandsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    brands = await _.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();
    brands.Should().NotBeNull();
    brands.Data.Should().NotBeNullOrEmpty();
    var newItemCount = brands.TotalCount;
    newItemCount.Should().Be(itemCount + 1);
    brands.Data.Should().Contain(a => a.Name == newBrand.Name);
  }

  [Fact]
  public async Task can_list_brand()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/brands/search", new SearchBrandsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var brands = await _.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();
    brands.Should().NotBeNull();
    brands.Data.Should().NotBeEmpty();
  }

  [Fact]
  public async Task can_update_brand_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/brands", new CreateBrandRequest
    {
      Name = "new brand",
      Description = ""
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var brandId = await _.Content.ReadFromJsonAsync<Guid>();

    var updateBrand = new UpdateBrandRequest
    {
      Id = brandId,
      Name = "new brand2",
      Description = ""
    };
    _ = await PutAsJsonAsync($"/api/v1/brands/{brandId}", updateBrand, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/brands/search", new SearchBrandsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var brands = await _.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();
    brands.Should().NotBeNull();
    brands.Data.Should().NotBeEmpty();
    var brand = brands.Data.FirstOrDefault(a => a.Id == brandId);
    brand.Should().NotBeNull();
    brand.Name.Should().Be(updateBrand.Name);
  }

  [Fact]
  public async Task can_delete_brand_when_brand_not_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();

    var _ = await PostAsJsonAsync("/api/v1/brands", new CreateBrandRequest
    {
      Name = "new brand",
      Description = ""
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var brandId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await DeleteAsJsonAsync($"/api/v1/brands/{brandId}", new DeleteBrandRequest(brandId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_delete_brand_when_brand_is_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/brands/search", new SearchBrandsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var brands = await _.Content.ReadFromJsonAsync<PaginationResponse<BrandDto>>();
    brands.Should().NotBeNull();
    brands.Data.Should().NotBeEmpty();
    var brand = brands.Data.First();

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.FirstOrDefault(a => a.BrandId == brand.Id);
    product.Should().NotBeNull();

    _ = await DeleteAsJsonAsync($"/api/v1/brands/{brand.Id}", new DeleteBrandRequest(brand.Id), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }
}