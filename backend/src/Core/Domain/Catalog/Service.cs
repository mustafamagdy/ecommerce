namespace FSH.WebApi.Domain.Catalog;

public class Service : AuditableEntity, IAggregateRoot, IHaveImageAndIcon
{
  public string Name { get; private set; }
  public string? Description { get; private set; }
  public string? ImagePath { get; private set; }
  public string? IconPath { get; private set; }
  public Guid ServiceCategoryId { get; private set; }
  public virtual ServiceCategory ServiceCategory { get; private set; } = default!;

  public Service(string name, string? description, Guid serviceCategoryId, string? imagePath, string? iconPath)
  {
    Name = name;
    Description = description;
    ImagePath = imagePath;
    IconPath = iconPath;
    ServiceCategoryId = serviceCategoryId;
  }

  public Service Update(string? name, string? description, Guid? serviceCategoryId, string? imagePath, string? iconPath)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (description is not null && Description?.Equals(description) is not true) Description = description;
    if (imagePath is not null && ImagePath?.Equals(imagePath) is not true) ImagePath = imagePath;
    if (iconPath is not null && IconPath?.Equals(iconPath) is not true) IconPath = iconPath;
    if (serviceCategoryId.HasValue && serviceCategoryId.Value != Guid.Empty && !ServiceCategoryId.Equals(serviceCategoryId.Value)) ServiceCategoryId = serviceCategoryId.Value;
    return this;
  }

  public Service ClearImagePath()
  {
    ImagePath = string.Empty;
    IconPath = string.Empty;
    return this;
  }
}