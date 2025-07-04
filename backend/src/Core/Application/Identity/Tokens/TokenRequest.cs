using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Identity.Tokens;

public record TokenRequest(string Email, string Password, Guid? BranchId = null);

public class TokenRequestValidator : CustomValidator<TokenRequest>
{
  public TokenRequestValidator(IStringLocalizer<TokenRequestValidator> T)
  {
    RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .EmailAddress()
      .WithMessage(T["Invalid Email Address."]);

    RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
      .NotEmpty();
  }
}