using Mapster;

namespace FSH.WebApi.Application.Catalog.Products;


/// <summary>
/// This is an example of how to use Dapper to execute SQL script directly
/// Be aware of which dialect you are using with your db vendor
/// </summary>
public class GetProductViaDapperRequest : IRequest<ProductDto>
{
    public Guid Id { get; set; }

    public GetProductViaDapperRequest(Guid id) => Id = id;
}

public class GetProductViaDapperRequestHandler : IRequestHandler<GetProductViaDapperRequest, ProductDto>
{
    private readonly IDapperEntityRepository _repository;
    private readonly IStringLocalizer _t;

    public GetProductViaDapperRequestHandler(IDapperEntityRepository repository, IStringLocalizer<GetProductViaDapperRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<ProductDto> Handle(GetProductViaDapperRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.QueryFirstOrDefaultAsync<Product>(
            $"SELECT * FROM public.\"Products\" WHERE \"Id\"  = '{request.Id}' AND \"Tenant\" = '@tenant'", cancellationToken: cancellationToken);

        _ = product ?? throw new NotFoundException(_t["Product {0} Not Found.", request.Id]);

        return product.Adapt<ProductDto>();
    }
}