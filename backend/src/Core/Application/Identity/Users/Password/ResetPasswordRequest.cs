namespace FSH.WebApi.Application.Identity.Users.Password;

public class ResetPasswordRequest
{
  public string? Email { get; set; }

  public string? Password { get; set; }

  public string? Token { get; set; }
}

public class UserResetPasswordRequest
{
  public Guid UserId { get; set; }
  public string? Password { get; set; }

  public UserResetPasswordRequest(Guid userId, string? password)
  {
    UserId = userId;
    Password = password;
  }
}