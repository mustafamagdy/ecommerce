using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class CreateBankAccountRequestValidator : CustomValidator<CreateBankAccountRequest>
{
    public CreateBankAccountRequestValidator()
    {
        RuleFor(p => p.AccountName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.AccountNumber)
            .NotEmpty()
            .MaximumLength(50);
            // Consider adding specific format validation if applicable for your region/bank

        RuleFor(p => p.BankName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.BranchName)
            .MaximumLength(100);

        RuleFor(p => p.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(p => p.GLAccountId)
            .NotEmptyGuid();
            // Add DB check in handler to ensure GLAccountId refers to a valid, active GL Account
    }
}
