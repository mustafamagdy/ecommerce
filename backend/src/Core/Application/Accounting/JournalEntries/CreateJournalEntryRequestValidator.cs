using FluentValidation;
using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.Accounting; // For TransactionType enum
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.JournalEntries;

public class CreateJournalEntryRequestValidator : CustomValidator<CreateJournalEntryRequest>
{
    public CreateJournalEntryRequestValidator()
    {
        RuleFor(p => p.EntryDate)
            .NotEmpty();

        RuleFor(p => p.Description)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(p => p.Transactions)
            .NotEmpty()
                .WithMessage("At least one transaction is required.")
            .Must(transactions =>
            {
                // Check for balanced debits and credits
                decimal totalDebits = transactions.Where(t =>
                    Enum.TryParse<TransactionType>(t.TransactionType, true, out var type) && type == TransactionType.Debit)
                    .Sum(t => t.Amount);
                decimal totalCredits = transactions.Where(t =>
                    Enum.TryParse<TransactionType>(t.TransactionType, true, out var type) && type == TransactionType.Credit)
                    .Sum(t => t.Amount);
                return totalDebits == totalCredits;
            })
                .WithMessage("Debits must equal Credits.");

        RuleForEach(p => p.Transactions).SetValidator(new CreateTransactionRequestItemValidator());
    }
}

public class CreateTransactionRequestItemValidator : CustomValidator<CreateTransactionRequestItem>
{
    public CreateTransactionRequestItemValidator()
    {
        RuleFor(t => t.AccountId)
            .NotEmpty();

        RuleFor(t => t.TransactionType)
            .NotEmpty()
            .IsEnumName(typeof(TransactionType), caseSensitive: false)
                .WithMessage("Invalid Transaction Type. Must be 'Debit' or 'Credit'.");

        RuleFor(t => t.Amount)
            .GreaterThan(0)
                .WithMessage("Amount must be positive.");

        RuleFor(t => t.Description)
            .MaximumLength(512);
    }
}
