namespace FSH.WebApi.Domain.Catalog;

public sealed class Product : AuditableEntity, IAggregateRoot, IHasImage
{
  public string Name { get; private set; }
  public string? Description { get; private set; }
  public decimal Rate { get; private set; }
  public string? ImagePath { get; private set; }
  public Guid BrandId { get; private set; }
  public Brand Brand { get; private set; } = default!;

  public Product(string name, string? description, decimal rate, Guid brandId, string? imagePath)
  {
    Name = name;
    Description = description;
    Rate = rate;
    ImagePath = imagePath;
    BrandId = brandId;
  }

  public Product Update(string? name, string? description, decimal? rate, Guid? brandId, string? imagePath)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    if (rate.HasValue && Rate != rate) Rate = rate.Value;
    if (brandId.HasValue && brandId.Value != Guid.Empty && !BrandId.Equals(brandId.Value)) BrandId = brandId.Value;
    if (imagePath is not null && ImagePath?.Equals(imagePath) is not true) ImagePath = imagePath;
    return this;
  }

  public Product ClearImagePath()
  {
    ImagePath = string.Empty;
    return this;
  }
}