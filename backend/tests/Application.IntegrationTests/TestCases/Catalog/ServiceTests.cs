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

public class ServiceTests : TestFixture
{
  public ServiceTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
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

}