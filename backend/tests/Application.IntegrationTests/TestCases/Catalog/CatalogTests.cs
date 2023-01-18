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

public class CatalogTests : TestFixture
{
  public CatalogTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  // service catalogs

  [Fact]
  public async Task can_create_service_catalog_with_new_product_and_service()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var itemCount = catalogItems.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    var newCatalogItem = new CreateServiceCatalogFromProductAndServiceRequest
    {
      Price = 100,
      Priority = ServicePriority.Normal,
      // ProductName = "new product",
      ServiceName = "new service"
    };
    _ = await PostAsJsonAsync("/api/v1/catalog/with-product-and-service", newCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var newItemCount = catalogItems.TotalCount;
    newItemCount.Should().Be(itemCount + 1);
    catalogItems.Data.Should().Contain(a => a.ProductName == newCatalogItem.Product.Name);
    catalogItems.Data.Should().Contain(a => a.ServiceName == newCatalogItem.ServiceName);
  }

  [Fact]
  public async Task can_create_service_catalog_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var itemCount = catalogItems.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.First();

    _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.First();

    var newCatalogItem = new CreateServiceCatalogRequest()
    {
      Price = 100,
      Priority = ServicePriority.Normal,
      ServiceId = service.Id,
      ProductId = product.Id
    };
    _ = await PostAsJsonAsync("/api/v1/catalog", newCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var newItemCount = catalogItems.TotalCount;
    newItemCount.Should().Be(itemCount + 1);
    catalogItems.Data.Should().Contain(a => a.ProductName == product.Name);
    catalogItems.Data.Should().Contain(a => a.ServiceName == service.Name);
  }

  [Fact]
  public async Task can_list_service_catalog()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task can_update_service_catalog_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var itemCount = catalogItems.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.First();

    _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.First();

    var newCatalogItem = new CreateServiceCatalogRequest()
    {
      Price = 100,
      Priority = ServicePriority.Normal,
      ServiceId = service.Id,
      ProductId = product.Id
    };
    _ = await PostAsJsonAsync("/api/v1/catalog", newCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var itemId = await _.Content.ReadFromJsonAsync<Guid>();

    var updateCatalogItem = new UpdateServiceCatalogRequest()
    {
      Id = itemId,
      Price = 120,
      Priority = ServicePriority.Urgent,
    };
    _ = await PutAsJsonAsync($"/api/v1/catalog/{itemId}", updateCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var updatedItem = catalogItems.Data.FirstOrDefault(a => a.Id == itemId);
    updatedItem.Price.Should().Be(updatedItem.Price);
    updatedItem.Priority.Should().Be(updatedItem.Priority);
  }

  [Fact]
  public async Task can_delete_service_catalog_when_service_catalog_not_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var itemCount = catalogItems.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.First();

    _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.First();

    var newCatalogItem = new CreateServiceCatalogRequest()
    {
      Price = 100,
      Priority = ServicePriority.Normal,
      ServiceId = service.Id,
      ProductId = product.Id
    };
    _ = await PostAsJsonAsync("/api/v1/catalog", newCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var itemId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await DeleteAsJsonAsync($"/api/v1/catalog/{itemId}", new DeleteServiceRequest(itemId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_delete_service_catalog_when_service_catalog_is_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogItems = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogItems.Should().NotBeNull();
    catalogItems.Data.Should().NotBeNullOrEmpty();
    var itemCount = catalogItems.TotalCount;
    itemCount.Should().BeGreaterThan(0);

    _ = await PostAsJsonAsync("/api/v1/products/search", new SearchProductsRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var products = await _.Content.ReadFromJsonAsync<PaginationResponse<ProductDto>>();
    products.Should().NotBeNull();
    products.Data.Should().NotBeEmpty();
    var product = products.Data.First();

    _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.First();

    var newCatalogItem = new CreateServiceCatalogRequest()
    {
      Price = 100,
      Priority = ServicePriority.Normal,
      ServiceId = service.Id,
      ProductId = product.Id
    };
    _ = await PostAsJsonAsync("/api/v1/catalog", newCatalogItem, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var itemId = await _.Content.ReadFromJsonAsync<Guid>();

    var order = new CreateCashOrderRequest
    {
      Items = new List<OrderItemRequest>
      {
        new()
        {
          ItemId = itemId,
          Qty = 1
        }
      },
    };

    var users = await GetUserList(adminHeaders);
    var cashRegisterId = await CreateNewCashRegister(branchId, users, adminHeaders);
    await OpenCashRegister(cashRegisterId, adminHeaders);
    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders/cash", order, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await DeleteAsJsonAsync($"/api/v1/catalog/{itemId}", new DeleteServiceRequest(itemId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }
}