using Microsoft.AspNetCore.Identity;

namespace FSH.WebApi.Domain.Identity;

public sealed class ApplicationUser : IdentityUser, IHasImage
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public bool Active { get; set; }
  public string? RefreshToken { get; set; }
  public DateTime RefreshTokenExpiryTime { get; set; }
  public Guid? LastUsedBranchId { get; set; }
  public string? ObjectId { get; set; }
  public bool MustChangePassword { get; set; }
  public string? ImagePath { get; private set; }

  public void SetAvatar(string? imagePath) => ImagePath = imagePath;

  public ApplicationUser ClearImagePath()
  {
    ImagePath = string.Empty;
    return this;
  }
}