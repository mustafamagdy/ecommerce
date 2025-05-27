using FluentValidation;
using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.Accounting; // Assuming AccountType enum is here
using System;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class CreateAccountRequestValidator : CustomValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(p => p.AccountName)
            .NotEmpty()
            .MaximumLength(256)
                .WithMessage("Account Name must not exceed 256 characters.");

        RuleFor(p => p.AccountNumber)
            .NotEmpty()
            .MaximumLength(50)
                .WithMessage("Account Number must not exceed 50 characters.");
        // Add more specific format validation for AccountNumber if needed later

        RuleFor(p => p.AccountType)
            .NotEmpty()
            .IsEnumName(typeof(AccountType), caseSensitive: false)
                .WithMessage("Invalid Account Type.");

        RuleFor(p => p.InitialBalance)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Initial Balance must not be negative.");
    }
}
