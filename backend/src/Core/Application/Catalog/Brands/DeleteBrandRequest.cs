using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Brands;

public class DeleteBrandRequest : IRequest<Guid>
{
  public Guid Id { get; set; }

  public DeleteBrandRequest(Guid id) => Id = id;
}

public class ProductsByBrandSpec : Specification<Product>
{
  public ProductsByBrandSpec(Guid brandId) =>
    Query.Where(p => p.BrandId == brandId);
}

public class DeleteBrandRequestHandler : IRequestHandler<DeleteBrandRequest, Guid>
{
  private readonly IRepositoryWithEvents<Brand> _brandRepo;
  private readonly IReadRepository<Product> _productRepo;

  private readonly IStringLocalizer _t;
  private readonly IApplicationUnitOfWork _uow;

  public DeleteBrandRequestHandler(IRepositoryWithEvents<Brand> brandRepo, IReadRepository<Product> productRepo,
    IStringLocalizer<DeleteBrandRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _brandRepo = brandRepo;
    _productRepo = productRepo;
    _t = localizer;
    _uow = uow;
  }

  public async Task<Guid> Handle(DeleteBrandRequest request, CancellationToken cancellationToken)
  {
    if (await _productRepo.AnyAsync(new ProductsByBrandSpec(request.Id), cancellationToken))
    {
      throw new ConflictException(_t["Brand cannot be deleted as it's being used."]);
    }

    var brand = await _brandRepo.GetByIdAsync(request.Id, cancellationToken);

    _ = brand ?? throw new NotFoundException(_t["Brand {0} Not Found.", request.Id]);

    if (brand.SystemDefault)
    {
      throw new ConflictException(_t["Default brand cannot be deleted."]);
    }

    brand.AddDomainEvent(EntityDeletedEvent.WithEntity(brand));

    await _brandRepo.DeleteAsync(brand, cancellationToken);

    await _uow.CommitAsync(cancellationToken);

    return request.Id;
  }
}