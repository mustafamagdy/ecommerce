using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class UpdateBankAccountRequestValidator : CustomValidator<UpdateBankAccountRequest>
{
    public UpdateBankAccountRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.AccountName)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.AccountName is not null);

        RuleFor(p => p.AccountNumber)
            .NotEmpty()
            .MaximumLength(50)
            .When(p => p.AccountNumber is not null);

        RuleFor(p => p.BankName)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.BankName is not null);

        RuleFor(p => p.BranchName)
            .MaximumLength(100)
            .When(p => p.BranchName is not null); // Allows empty string if explicitly set

        RuleFor(p => p.Currency)
            .NotEmpty()
            .Length(3)
            .When(p => p.Currency is not null);

        RuleFor(p => p.GLAccountId)
            .NotEmptyGuid()
            .When(p => p.GLAccountId.HasValue);
    }
}
