namespace FSH.WebApi.Domain.Catalog;

public class Service : AuditableEntity, IAggregateRoot, IHaveImage
{
  private Service()
  {
  }

  public string Name { get; set; }
  public string? Description { get; set; }
  public string? ImageUrl { get; set; }

  public Service(string name, string? description, string? imageUrl)
  {
    Name = name;
    Description = description;
    ImageUrl = imageUrl;
  }
}