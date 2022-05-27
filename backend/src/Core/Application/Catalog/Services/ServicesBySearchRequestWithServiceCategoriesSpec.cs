namespace FSH.WebApi.Application.Catalog.Services;

public class ServicesBySearchRequestWithServiceCategoriesSpec : EntitiesByPaginationFilterSpec<Service, ServiceDto>
{
  public ServicesBySearchRequestWithServiceCategoriesSpec(SearchServicesRequest request)
    : base(request) =>
    Query
      .Include(p => p.ServiceCategory)
      .OrderBy(c => c.Name, !request.HasOrderBy())
      .Where(p => p.ServiceCategoryId.Equals(request.ServiceCategoryId!.Value), request.ServiceCategoryId.HasValue);
}