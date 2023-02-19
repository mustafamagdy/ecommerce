namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public record GetServiceCatalogByIdRequest(Guid Id) : IRequest<ServiceCatalogDto>
{
}

public class GetServiceCatalogByIdRequestSpec : Specification<ServiceCatalog, ServiceCatalogDto>,
    ISingleResultSpecification
{
    public GetServiceCatalogByIdRequestSpec(Guid id) => Query.Include(a => a.Service).Include(a => a.Product)
        .Where(s => s.Id == id);
}

public class GetServiceCatalogByIdRequestHandler : IRequestHandler<GetServiceCatalogByIdRequest, ServiceCatalogDto>
{
    private readonly IReadRepository<ServiceCatalog> _repository;

    public GetServiceCatalogByIdRequestHandler(IReadRepository<ServiceCatalog> repository) => _repository = repository;

    public async Task<ServiceCatalogDto> Handle(GetServiceCatalogByIdRequest request, CancellationToken
        cancellationToken)
        => await _repository
            .GetBySpecAsync(new GetServiceCatalogByIdRequestSpec(request.Id), cancellationToken);
}