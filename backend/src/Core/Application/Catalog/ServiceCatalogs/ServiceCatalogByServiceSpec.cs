namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public sealed class ServiceCatalogByServiceSpec : Specification<ServiceCatalog>, ISingleResultSpecification
{
  public ServiceCatalogByServiceSpec(Guid serviceId) => Query.Where(b => b.ServiceId == serviceId);
}