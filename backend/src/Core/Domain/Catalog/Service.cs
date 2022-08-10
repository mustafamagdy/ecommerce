namespace FSH.WebApi.Domain.Catalog;

public class Service : AuditableEntity, IAggregateRoot, IHaveImage
{
  private Service()
  {
  }

  public Service(string name, string? description, string? imageUrl)
  {
    Name = name;
    Description = description;
    ImageUrl = imageUrl;
  }

  public string Name { get; private set; }
  public string? Description { get; private set; }
  public string? ImageUrl { get; private set; }
  public void SetImageUrl(string imageUrl) => ImageUrl = imageUrl;
}