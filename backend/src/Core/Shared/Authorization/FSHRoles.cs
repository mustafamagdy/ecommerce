using System.Collections.ObjectModel;

namespace FSH.WebApi.Shared.Authorization;

public static class FSHRoles
{
  public const string Admin = nameof(Admin);
  public const string Basic = nameof(Basic);
  public const string Demo = nameof(Demo);

  public static IReadOnlyCollection<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
  {
    Admin,
    Basic,
    Demo
  });

  public static bool IsDefault(string roleName) => DefaultRoles.Any(r => r == roleName);
}