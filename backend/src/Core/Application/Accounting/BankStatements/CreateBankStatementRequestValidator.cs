using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using FSH.WebApi.Domain.Accounting; // For BankTransactionType
using System.Linq; // For Sum

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class CreateBankStatementRequestValidator : CustomValidator<CreateBankStatementRequest>
{
    public CreateBankStatementRequestValidator()
    {
        RuleFor(p => p.BankAccountId)
            .NotEmptyGuid();

        RuleFor(p => p.StatementDate)
            .NotEmpty();

        // OpeningBalance can be positive, negative or zero.
        // ClosingBalance can be positive, negative or zero.

        // Basic validation: sum of transactions should roughly reconcile opening and closing balances.
        // (ClosingBalance - OpeningBalance) == Sum of credits - Sum of debits
        RuleFor(p => p)
            .Must(p => {
                decimal creditSum = p.Transactions.Where(t => t.Type == BankTransactionType.Credit).Sum(t => t.Amount);
                decimal debitSum = p.Transactions.Where(t => t.Type == BankTransactionType.Debit).Sum(t => t.Amount);
                // Using a small tolerance for decimal comparisons
                return Math.Abs((p.ClosingBalance - p.OpeningBalance) - (creditSum - debitSum)) < 0.001m;
            })
            .WithMessage("Closing balance, opening balance, and transaction totals do not reconcile.")
            .When(p => p.Transactions != null && p.Transactions.Any());


        RuleFor(p => p.ReferenceNumber)
            .MaximumLength(100);

        RuleForEach(p => p.Transactions)
            .SetValidator(new CreateBankStatementTransactionRequestItemValidator());
    }
}

public class CreateBankStatementTransactionRequestItemValidator : CustomValidator<CreateBankStatementTransactionRequestItem>
{
    public CreateBankStatementTransactionRequestItemValidator()
    {
        RuleFor(i => i.TransactionDate)
            .NotEmpty();

        RuleFor(i => i.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(i => i.Amount)
            .GreaterThan(0);

        RuleFor(i => i.Type)
            .IsInEnum(); // Ensures it's a valid BankTransactionType value

        RuleFor(i => i.Reference)
            .MaximumLength(100);

        RuleFor(i => i.BankProvidedId)
            .MaximumLength(100);
    }
}
