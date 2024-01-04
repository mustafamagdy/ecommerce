using System.Net;
using System.Net.Http.Json;
using Application.IntegrationTests.Infra;
using FluentAssertions;
using FSH.WebApi.Application.Catalog.Categories;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Application.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace Application.IntegrationTests.TestCases.Catalog;

public class CategoryTests : TestFixture
{
  public CategoryTests(HostFixture host, ITestOutputHelper output)
    : base(host, output)
  {
  }

  // categories
  [Fact]
  public async Task can_list_category()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/categories/search", new SearchCategoriesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var categories = await _.Content.ReadFromJsonAsync<PaginationResponse<CategoryDto>>();
    categories.Should().NotBeNull();
    categories.Data.Should().NotBeEmpty();
  }

  [Fact]
  public async Task can_update_category_when_submit_valid_data()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest
    {
      Name = "new category",
      Description = "",
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var categoryId = await _.Content.ReadFromJsonAsync<Guid>();

    var updateCategory = new UpdateCategoryRequest
    {
      Id = categoryId,
      Name = "new category2",
      Description = ""
    };
    _ = await PutAsJsonAsync($"/api/v1/categories/{categoryId}", updateCategory, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    _ = await PostAsJsonAsync("/api/v1/categories/search", new SearchCategoriesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var categories = await _.Content.ReadFromJsonAsync<PaginationResponse<CategoryDto>>();
    categories.Should().NotBeNull();
    categories.Data.Should().NotBeEmpty();
    var category = categories.Data.FirstOrDefault(a => a.Id == categoryId);
    category.Should().NotBeNull();
    category.Name.Should().Be(updateCategory.Name);
  }

  [Fact]
  public async Task can_delete_category_when_category_not_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();

    var _ = await PostAsJsonAsync("/api/v1/categories", new CreateCategoryRequest
    {
      Name = "new category",
      Description = ""
    }, adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var categoryId = await _.Content.ReadFromJsonAsync<Guid>();

    _ = await DeleteAsJsonAsync($"/api/v1/categories/{categoryId}", new DeleteCategoryRequest(categoryId),
      adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task cannot_delete_category_when_category_is_used()
  {
    var (adminHeaders, branchId) = await CreateTenantAndLogin();
    var _ = await PostAsJsonAsync("/api/v1/categories/search", new SearchCategoriesRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);
    var categories = await _.Content.ReadFromJsonAsync<PaginationResponse<CategoryDto>>();
    categories.Should().NotBeNull();
    categories.Data.Should().NotBeEmpty();
    var category = categories.Data.First();

    _ = await PostAsJsonAsync("/api/v1/catalog/search", new SearchServiceCatalogRequest(), adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.OK);

    var catalogs = await _.Content.ReadFromJsonAsync<PaginationResponse<ServiceCatalogDto>>();
    catalogs.Should().NotBeNull();
    catalogs.Data.Should().NotBeEmpty();
    var catalogItem = catalogs.Data.FirstOrDefault(a => a.CategoryName == category.Name);
    catalogItem.Should().NotBeNull();

    _ = await DeleteAsJsonAsync($"/api/v1/categories/{category.Id}", new DeleteCategoryRequest(category.Id),
      adminHeaders);
    _.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }
}