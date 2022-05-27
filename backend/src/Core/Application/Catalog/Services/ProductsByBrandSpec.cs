namespace FSH.WebApi.Application.Catalog.Services;

public class ServicesByServiceCategorySpec : Specification<Service>
{
  public ServicesByServiceCategorySpec(Guid serviceCategoryId) =>
    Query.Where(p => p.ServiceCategoryId == serviceCategoryId);
}