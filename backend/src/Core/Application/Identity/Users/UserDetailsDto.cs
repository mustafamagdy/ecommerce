using FSH.WebApi.Domain.Identity;

namespace FSH.WebApi.Application.Identity.Users;

public sealed class UserWithRole
{
  public ApplicationUser User { get; set; }
  public List<ApplicationRole> Roles { get; set; }
}
public class BasicUserDataDto : IDto
{
  public Guid Id { get; set; }
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string? Email { get; set; }
  public List<string> Roles { get; set; }
  public bool Active { get; set; }
  public string FullName => $"{FirstName} {LastName}";
}

public class UserDetailsDto : BasicUserDataDto
{
  public string? UserName { get; set; }
  public bool EmailConfirmed { get; set; }
  public string? PhoneNumber { get; set; }
  public string? ImagePath { get; set; }


  public string Status {
    get
    {
      if (!EmailConfirmed)
      {
        return "pending";
      }

      return !Active ? "inactive" : "active";
    }
  }
}

public class CreateUserResponseDto : UserDetailsDto
{
  public string? CreateUserMessages { get; set; }
}