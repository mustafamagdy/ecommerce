using FSH.WebApi.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
internal static class Startup
{
    internal static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration config)
    {
      var identityOpt = config.GetSection(nameof(IdentitySettings)).Get<IdentitySettings>();
      return services
        .AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
          options.Password.RequiredLength = identityOpt.PasswordMinLength;
          options.Password.RequireDigit = identityOpt.PasswordRequireDigit;
          options.Password.RequireLowercase = identityOpt.PasswordRequireLowercase;
          options.Password.RequireNonAlphanumeric = identityOpt.PasswordRequireNonAlphanumeric;
          options.Password.RequireUppercase = identityOpt.PasswordRequireUppercase;
          options.User.RequireUniqueEmail = identityOpt.UserRequireUniqueEmail;
          options.SignIn.RequireConfirmedEmail = identityOpt.SignInRequireConfirmedEmail;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .Services;
    }
}