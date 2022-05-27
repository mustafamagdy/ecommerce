namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceByNameSpec : Specification<Service>, ISingleResultSpecification
{
    public ServiceByNameSpec(string name) =>
        Query.Where(p => p.Name == name);
}