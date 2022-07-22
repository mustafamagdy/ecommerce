namespace FSH.WebApi.Infrastructure.Identity;

public class IdentitySettings
{
  public int PasswordMinLength { get; set; }
  public bool PasswordRequireDigit { get; set; }
  public bool PasswordRequireLowercase { get; set; }
  public bool PasswordRequireNonAlphanumeric { get; set; }
  public bool PasswordRequireUppercase { get; set; }
  public bool UserRequireUniqueEmail { get; set; }
  public bool SignInRequireConfirmedEmail { get; set; }
  public bool RequireConfirmedAccount { get; set; }
}