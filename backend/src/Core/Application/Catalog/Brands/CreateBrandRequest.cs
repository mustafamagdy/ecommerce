using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Brands;

public class CreateBrandRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
}

public sealed class BrandByNameSpec : Specification<Brand>, ISingleResultSpecification
{
  public BrandByNameSpec(string name) => Query.Where(b => b.Name == name);
}

public class CreateBrandRequestValidator : CustomValidator<CreateBrandRequest>
{
  public CreateBrandRequestValidator(IReadRepository<Brand> repository, IStringLocalizer<CreateBrandRequestValidator> T)
    =>
      RuleFor(p => p.Name)
        .NotEmpty()
        .MaximumLength(75)
        .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new BrandByNameSpec(name), ct) is null)
        .WithMessage((_, name) => T["Brand {0} already Exists.", name]);
}

public class CreateBrandRequestHandler : IRequestHandler<CreateBrandRequest, Guid>
{
  private readonly IRepositoryWithEvents<Brand> _repository;
  private readonly IApplicationUnitOfWork _uow;

  public CreateBrandRequestHandler(IRepositoryWithEvents<Brand> repository, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateBrandRequest request, CancellationToken cancellationToken)
  {
    var brand = new Brand(request.Name, request.Description);

    await _repository.AddAsync(brand, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return brand.Id;
  }
}