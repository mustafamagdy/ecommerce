﻿namespace FSH.WebApi.Application.Catalog.Products;

public class GetProductRequest : IRequest<ProductDetailsDto>
{
    public Guid Id { get; set; }

    public GetProductRequest(Guid id) => Id = id;
}

public class ProductByIdWithBrandSpec : Specification<Product, ProductDetailsDto>, ISingleResultSpecification
{
  public ProductByIdWithBrandSpec(Guid id) =>
    Query
      .Where(p => p.Id == id)
      .Include(p => p.Brand);
}
public class GetProductRequestHandler : IRequestHandler<GetProductRequest, ProductDetailsDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IStringLocalizer _t;

    public GetProductRequestHandler(IRepository<Product> repository, IStringLocalizer<GetProductRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<ProductDetailsDto> Handle(GetProductRequest request, CancellationToken cancellationToken) =>
        await _repository.FirstOrDefaultAsync(
            new ProductByIdWithBrandSpec(request.Id), cancellationToken)
        ?? throw new NotFoundException(_t["Product {0} Not Found.", request.Id]);
}