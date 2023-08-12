using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Products;

public class UpdateProductRequest : IRequest<Guid>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public decimal? Rate { get; set; }
  public Guid? BrandId { get; set; }
  public bool DeleteCurrentImage { get; set; } = false;
  public FileUploadRequest? Image { get; set; }
}

public class UpdateProductRequestValidator : CustomValidator<UpdateProductRequest>
{
  public UpdateProductRequestValidator(IReadRepository<Product> productRepo, IReadRepository<Brand> brandRepo, IStringLocalizer<UpdateProductRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (product, name, ct) =>
        await productRepo.FirstOrDefaultAsync(new ProductByNameSpec(name), ct)
          is not { } existingProduct || existingProduct.Id == product.Id)
      .WithMessage((_, name) => T["Product {0} already Exists.", name]);

    RuleFor(p => p.Rate)
      .GreaterThanOrEqualTo(0)
      .When(a => a.Rate != null);

    RuleFor(p => p.Image)
      .InjectValidator();

    RuleFor(p => p.BrandId)
      .MustAsync(async (id, ct) => await brandRepo.GetByIdAsync(id, ct) is not null)
      .When(a => a.BrandId != null)
      .WithMessage((_, id) => T["Brand {0} Not Found.", id]);
  }
}

public class UpdateProductRequestHandler : IRequestHandler<UpdateProductRequest, Guid>
{
  private readonly IRepository<Product> _repository;
  private readonly IStringLocalizer _t;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateProductRequestHandler(IRepository<Product> repository, IStringLocalizer<UpdateProductRequestHandler> localizer, IFileStorageService file, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
    _t = localizer;
  }

  public async Task<Guid> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
  {
    var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

    _ = product ?? throw new NotFoundException(_t["Product {0} Not Found.", request.Id]);

    // Remove old image if flag is set
    if (request.DeleteCurrentImage)
    {
      string? currentProductImagePath = product.ImagePath;
      if (!string.IsNullOrEmpty(currentProductImagePath))
      {
        string root = Directory.GetCurrentDirectory();
        _file.Remove(Path.Combine(root, currentProductImagePath));
      }

      product = product.ClearImagePath();
    }

    string? productImagePath = request.Image is not null
      ? await _file.UploadAsync<Product>(request.Image, FileType.Image, cancellationToken)
      : null;

    var updatedProduct = product.Update(request.Name, request.Description, request.Rate, request.BrandId, productImagePath);

    product.AddDomainEvent(EntityUpdatedEvent.WithEntity(product));

    await _repository.UpdateAsync(updatedProduct, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return request.Id;
  }
}