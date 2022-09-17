namespace FSH.WebApi.Domain.Catalog;

public sealed class Service : AuditableEntity, IAggregateRoot, IHaveImage
{
  private Service()
  {
  }

  public Service(string name, string? description, string? imagePath)
  {
    Name = name;
    Description = description;
    ImagePath = imagePath;
  }

  public string Name { get; private set; }
  public string? Description { get; private set; }
  public string? ImagePath { get; private set; }
  public void SetImageUrl(string imagePath) => ImagePath = imagePath;

  public Service Update(string? name, string? description,  string? imagePath)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    if (imagePath is not null && ImagePath?.Equals(imagePath) is not true) ImagePath = imagePath;
    return this;
  }
}