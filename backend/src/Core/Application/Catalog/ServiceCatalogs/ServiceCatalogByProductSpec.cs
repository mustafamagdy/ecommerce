namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public sealed class ServiceCatalogByProductSpec : Specification<ServiceCatalog>, ISingleResultSpecification
{
  public ServiceCatalogByProductSpec(Guid productId) => Query.Where(b => b.ProductId == productId);
}