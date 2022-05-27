namespace FSH.WebApi.Domain.Catalog;

public class ServiceCategory : AuditableEntity, IAggregateRoot, IHaveImageAndIcon
{
  public string Name { get; private set; }
  public string? Description { get; private set; }
  public string? ImagePath { get; private set; }
  public string? IconPath { get; private set; }

  public ServiceCategory(string name, string? description, string? imagePath, string? iconPath)
  {
    Name = name;
    Description = description;
    ImagePath = imagePath;
    IconPath = iconPath;
  }

  public ServiceCategory Update(string? name, string? description, string? imagePath, string? iconPath)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    if (imagePath is not null && ImagePath?.Equals(imagePath) is not true) ImagePath = imagePath;
    if (iconPath is not null && IconPath?.Equals(iconPath) is not true) IconPath = iconPath;
    return this;
  }
}