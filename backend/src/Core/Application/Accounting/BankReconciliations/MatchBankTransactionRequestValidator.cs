using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class MatchBankTransactionRequestValidator : CustomValidator<MatchBankTransactionRequest>
{
    public MatchBankTransactionRequestValidator()
    {
        RuleFor(p => p.BankReconciliationId)
            .NotEmptyGuid();

        RuleFor(p => p.BankStatementTransactionId)
            .NotEmptyGuid();

        // If IsMatched is true, then SystemTransactionId and SystemTransactionType should be provided.
        RuleFor(p => p.SystemTransactionId)
            .NotEmptyGuid()
            .When(p => p.IsMatched && p.SystemTransactionId.HasValue) // Check NotEmptyGuid only if it has value and is matched
            .WithMessage("System Transaction ID is required when matching.")
            .When(p => p.IsMatched);


        RuleFor(p => p.SystemTransactionType)
            .NotEmpty()
            .MaximumLength(50)
            .When(p => p.IsMatched && !string.IsNullOrEmpty(p.SystemTransactionType)) // Check NotEmpty only if it has value and is matched
            .WithMessage("System Transaction Type is required when matching.")
            .When(p => p.IsMatched);


        // If IsMatched is false (unmatching), SystemTransactionId and SystemTransactionType might not be relevant or could be null.
        // The handler will primarily use BankStatementTransactionId to unmark it.
    }
}
