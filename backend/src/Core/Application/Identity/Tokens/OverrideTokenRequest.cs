namespace FSH.WebApi.Application.Identity.Tokens;

public record OverrideTokenRequest(string Email, string Password, string Permission, object Scope);

public record ManagerOverrideToken(string Permission, string Scope);

public class OverrideTokenRequestValidator : CustomValidator<OverrideTokenRequest>
{
  public OverrideTokenRequestValidator(IStringLocalizer<TokenRequestValidator> T)
  {
    RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .EmailAddress()
      .WithMessage(T["Invalid Email Address."]);

    RuleFor(p => p.Password).Cascade(CascadeMode.Stop)
      .NotEmpty();

    RuleFor(p => p.Permission).Cascade(CascadeMode.Stop)
      .NotEmpty();

    RuleFor(p => p.Scope).Cascade(CascadeMode.Stop)
      .NotNull();
  }
}