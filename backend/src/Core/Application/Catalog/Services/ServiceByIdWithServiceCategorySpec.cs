namespace FSH.WebApi.Application.Catalog.Services;

public class ServiceByIdWithServiceCategorySpec : Specification<Service, ServiceDetailsDto>, ISingleResultSpecification
{
    public ServiceByIdWithServiceCategorySpec(Guid id) =>
        Query
            .Where(p => p.Id == id)
            .Include(p => p.ServiceCategory);
}