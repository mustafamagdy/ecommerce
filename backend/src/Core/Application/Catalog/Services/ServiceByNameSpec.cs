namespace FSH.WebApi.Application.Catalog.Services;

public sealed class ServiceByNameSpec : Specification<Service>, ISingleResultSpecification
{
  public ServiceByNameSpec(string name) => Query.Where(b => b.Name == name);
}