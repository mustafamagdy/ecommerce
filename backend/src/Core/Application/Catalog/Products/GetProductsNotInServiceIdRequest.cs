using Mapster;

namespace FSH.WebApi.Application.Catalog.Products;

public class GetProductsNotInServiceIdRequest : PaginationFilter, IRequest<PaginationResponse<ProductDto>>
{
    public Guid ServiceId { get; set; }
}

public class
    GetProductsNotInServiceIdRequestHandler : IRequestHandler<GetProductsNotInServiceIdRequest,
        PaginationResponse<ProductDto>>
{
    private readonly IReadRepository<Product> _repository;
    private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepository;
    public GetProductsNotInServiceIdRequestHandler(IReadRepository<Product> repository, IReadRepository<ServiceCatalog> serviceCatalogRepository)
    {
        _repository = repository;
        _serviceCatalogRepository = serviceCatalogRepository;
    }

    public async Task<PaginationResponse<ProductDto>> Handle(GetProductsNotInServiceIdRequest request,
        CancellationToken cancellationToken)
    {
        var serviceCatalogs = await _serviceCatalogRepository.ListAsync(new ServiceCatalogById(request), cancellationToken);
        var result = await _repository.PaginatedListAsync(
            new ProductsNotInServiceIdSpec(request,serviceCatalogs.Select(sc=>sc.ProductId).ToList()),
            request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
        return result;
    }
}

public class ServiceCatalogById : Specification<ServiceCatalog>
{
    public ServiceCatalogById(GetProductsNotInServiceIdRequest request) =>
        this.Query
            .Where(p => p.ServiceId == request.ServiceId);


}
public class ProductsNotInServiceIdSpec : EntitiesByPaginationFilterSpec<Product, ProductDto>
{
    public ProductsNotInServiceIdSpec(GetProductsNotInServiceIdRequest request,List<Guid> productIds) : base(request) =>
        this.Query
            .Where(p => !productIds.Contains(p.Id));
}