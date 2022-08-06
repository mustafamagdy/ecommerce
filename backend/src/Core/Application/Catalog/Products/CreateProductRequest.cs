using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Catalog.Products;

public class CreateProductRequest : IRequest<Guid>
{
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public decimal Rate { get; set; }
  public Guid BrandId { get; set; }
  public FileUploadRequest? Image { get; set; }
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, Guid>
{
  private readonly IRepository<Product> _repository;
  private readonly IFileStorageService _file;
  private readonly IApplicationUnitOfWork _uow;

  public CreateProductRequestHandler(IRepository<Product> repository, IFileStorageService file, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _file = file;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateProductRequest request, CancellationToken cancellationToken)
  {
    string productImagePath = await _file.UploadAsync<Product>(request.Image, FileType.Image, cancellationToken);

    var product = new Product(request.Name, request.Description, request.Rate, request.BrandId, productImagePath);

    // Add Domain Events to be raised after the commit
    product.AddDomainEvent(EntityCreatedEvent.WithEntity(product));

    await _repository.AddAsync(product, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return product.Id;
  }
}