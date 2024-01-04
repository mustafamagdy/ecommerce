namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public sealed class ServiceCatalogByCategorySpec : Specification<ServiceCatalog>, ISingleResultSpecification
{
  public ServiceCatalogByCategorySpec(Guid categoryId) => Query.Where(b => b.CategoryId == categoryId);
}