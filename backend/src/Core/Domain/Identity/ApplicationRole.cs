using Microsoft.AspNetCore.Identity;

namespace FSH.WebApi.Domain.Identity;

public sealed class ApplicationRole : IdentityRole
{
  public string? Description { get; set; }

  public ApplicationRole(string name, string? description = null)
    : base(name)
  {
    Description = description;
    NormalizedName = name.ToUpperInvariant();
  }
}