using FluentValidation;
using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.Accounting; // Assuming AccountType enum is here
using System;

namespace FSH.WebApi.Application.Accounting.Accounts;

public class UpdateAccountRequestValidator : CustomValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.AccountName)
            .MaximumLength(256)
                .WithMessage("Account Name must not exceed 256 characters.")
            .When(p => p.AccountName is not null); // Only validate if provided

        RuleFor(p => p.AccountNumber)
            .MaximumLength(50)
                .WithMessage("Account Number must not exceed 50 characters.")
            .When(p => p.AccountNumber is not null); // Only validate if provided

        RuleFor(p => p.AccountType)
            .IsEnumName(typeof(AccountType), caseSensitive: false)
                .WithMessage("Invalid Account Type.")
            .When(p => p.AccountType is not null); // Only validate if provided
    }
}
