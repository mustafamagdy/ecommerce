using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Domain.Catalog;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Catalog;

public class CatalogTests : TestFixture
{
  public CatalogTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  // brands

  [Fact]
  public async Task can_create_brand_when_submit_valid_data_and_has_permission()
  {
    var adminHeaders = await CreateTenantAndLogin();
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
    newItemCount.Should().BeGreaterThan(0);
    newItemCount.Should().BeGreaterThan(itemCount);
    brands.Data.Should().Contain(a => a.Name == newBrand.Name);
  }

  [Fact]
  public async Task can_list_brand_when__has_permission()
  {
  }

  [Fact]
  public async Task can_update_brand_when_submit_valid_data_and_has_permission()
  {
  }

  [Fact]
  public async Task can_delete_brand_when_brand_not_used_and_has_permission()
  {
  }

  [Fact]
  public async Task cannot_delete_brand_when_brand_is_used_and_has_permission()
  {
  }

  /*
   * // products
   * can_create_product_when_submit_valid_data_and_has_permission
   * can_list_product_when__has_permission
   * can_update_product_when_submit_valid_data_and_has_permission
   * can_delete_product_when_product_not_used_and_has_permission
   * cannot_delete_product_when_product_is_used_and_has_permission
   * // services
   * can_create_service_when_submit_valid_data_and_has_permission
   * can_list_service_when__has_permission
   * can_update_service_when_submit_valid_data_and_has_permission
   * can_delete_service_when_service_not_used_and_has_permission
   * cannot_delete_service_when_service_is_used_and_has_permission
   * // service catalogs
   * can_create_service_catalog_when_submit_valid_data_and_has_permission
   * can_list_service_catalog_when__has_permission
   * can_update_service_catalog_when_submit_valid_data_and_has_permission
   * can_delete_service_catalog_when_service_catalog_not_used_and_has_permission
   * cannot_delete_service_catalog_when_service_catalog_is_used_and_has_permission
   */
}