using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Brands;

public class GetBrandRequest : IRequest<BrandDto>
{
  public Guid Id { get; set; }

  public GetBrandRequest(Guid id) => Id = id;
}

public class BrandByIdSpec : Specification<Brand, BrandDto>, ISingleResultSpecification
{
  public BrandByIdSpec(Guid id) =>
    Query.Where(p => p.Id == id);
}

public class GetBrandRequestHandler : IRequestHandler<GetBrandRequest, BrandDto>
{
  private readonly IReadRepository<Brand> _repository;
  private readonly IStringLocalizer _t;

  public GetBrandRequestHandler(IReadRepository<Brand> repository, IStringLocalizer<GetBrandRequestHandler> localizer)
  {
    _repository = repository;
    _t = localizer;
  }

  public async Task<BrandDto> Handle(GetBrandRequest request, CancellationToken cancellationToken) =>
    await _repository.GetBySpecAsync(new BrandByIdSpec(request.Id), cancellationToken)
    ?? throw new NotFoundException(_t["Brand {0} Not Found.", request.Id]);
}