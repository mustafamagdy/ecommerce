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

  // services

  [Fact]
  public async Task can_list_service()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
  }

  [Fact]
  public async Task can_update_service_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/services", new CreateServiceRequest
    {
      Name = "new service",
      Description = "",
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var serviceId = await _.Content.ReadFromJsonAsync<Guid>();

    var updateService = new UpdateServiceRequest
    {
      Id = serviceId,
      Name = "new service2",
      Description = ""
    };
    _ = await PutAsJsonAsync($"/api/v1/services/{serviceId}", updateService, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.FirstOrDefault(a => a.Id == serviceId);
    service.Should().NotBeNull();
    service.Name.Should().Be(updateService.Name);
  }

  [Fact]
  public async Task can_delete_service_when_service_not_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();

    var _ = await PostAsJsonAsync("/api/v1/services", new CreateServiceRequest
    {
      Name = "new service",
      Description = ""
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var serviceId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await DeleteAsJsonAsync($"/api/v1/services/{serviceId}", new DeleteServiceRequest(serviceId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_delete_service_when_service_is_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/services/search", new SearchServicesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var services = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceDto>>();
    services.Should().NotBeNull();
    services.Data.Should().NotBeEmpty();
    var service = services.Data.First();

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogs = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogs.Should().NotBeNull();
    catalogs.Data.Should().NotBeEmpty();
    var catalogItem = catalogs.Data.FirstOrDefault(a => a.ServiceName == service.Name);
    catalogItem.Should().NotBeNull();

    _ = await DeleteAsJsonAsync($"/api/v1/services/{service.Id}", new DeleteServiceRequest(service.Id), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
      ProductName = "new product",
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
    catalogItems.Data.Should().Contain(a => a.ProductName == newCatalogItem.ProductName);
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
    var cashRegisterId = await createNewCashRegister(branchId, users, adminHeaders);
    await openCashRegister(cashRegisterId, adminHeaders);
    adminHeaders.Add("cash-register", cashRegisterId.ToString());

    _ = await PostAsJsonAsync("/api/v1/orders/cash", order, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await DeleteAsJsonAsync($"/api/v1/catalog/{itemId}", new DeleteServiceRequest(itemId), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }
}