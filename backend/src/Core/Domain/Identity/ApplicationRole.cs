using Microsoft.AspNetCore.Identity;

namespace FSH.WebApi.Domain.Identity;

public sealed class ApplicationRole : IdentityRole
{
  public string? Description { get; set; }

  public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

  public ApplicationRole(string name, string? description = null)
    : base(name)
  {
    Description = description;
    NormalizedName = name.ToUpperInvariant();
  }
}

public class ApplicationUserRole : IdentityUserRole<string>
{
  public virtual ApplicationUser User { get; set; }
  public virtual ApplicationRole Role { get; set; }
}