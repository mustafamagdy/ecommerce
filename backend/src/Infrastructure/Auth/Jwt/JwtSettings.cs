using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Infrastructure.Auth.Jwt;

public class JwtSettings : IValidatableObject
{
  public string Key { get; set; } = string.Empty;
  public string OverrideTokenKey { get; set; } = string.Empty;

  public int TokenExpirationInMinutes { get; set; }

  public int OverrideTokenExpirationInMinutes { get; set; }

  public int RefreshTokenExpirationInDays { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (string.IsNullOrEmpty(Key))
    {
      yield return new ValidationResult("No Key defined in JwtSettings config", new[] { nameof(Key) });
    }

    if (string.IsNullOrEmpty(OverrideTokenKey))
    {
      yield return new ValidationResult("No Override Token Key defined in JwtSettings config", new[] { nameof(OverrideTokenKey) });
    }
  }
}