namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public sealed class ServiceCategoryByNameSpec : Specification<ServiceCategory>, ISingleResultSpecification
{
  public ServiceCategoryByNameSpec(string name) => Query.Where(b => b.Name == name);
}