namespace FSH.WebApi.Application.Catalog.ServiceCategories;

public class GetServiceCategoryRequest : IRequest<ServiceCategoryDto>
{
    public Guid Id { get; set; }

    public GetServiceCategoryRequest(Guid id) => Id = id;
}

public class ServiceCategoryByIdSpec : Specification<ServiceCategory, ServiceCategoryDto>, ISingleResultSpecification
{
    public ServiceCategoryByIdSpec(Guid id) =>
        Query.Where(p => p.Id == id);
}

public class GetServiceCategoryRequestHandler : IRequestHandler<GetServiceCategoryRequest, ServiceCategoryDto>
{
    private readonly IRepository<ServiceCategory> _repository;
    private readonly IStringLocalizer _t;

    public GetServiceCategoryRequestHandler(IRepository<ServiceCategory> repository, IStringLocalizer<GetServiceCategoryRequestHandler>
    localizer) =>
    (_repository, _t) = (repository, localizer);

    public async Task<ServiceCategoryDto> Handle(GetServiceCategoryRequest request, CancellationToken cancellationToken) =>
        await _repository.GetBySpecAsync(
            (ISpecification<ServiceCategory, ServiceCategoryDto>)new ServiceCategoryByIdSpec(request.Id), cancellationToken)
        ?? throw new NotFoundException(_t["ServiceCategory {0} Not Found.", request.Id]);
}