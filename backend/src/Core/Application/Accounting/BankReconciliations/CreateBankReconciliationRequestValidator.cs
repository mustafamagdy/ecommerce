using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class CreateBankReconciliationRequestValidator : CustomValidator<CreateBankReconciliationRequest>
{
    public CreateBankReconciliationRequestValidator()
    {
        RuleFor(p => p.BankAccountId)
            .NotEmptyGuid();

        RuleFor(p => p.ReconciliationDate)
            .NotEmpty();

        RuleFor(p => p.BankStatementId)
            .NotEmptyGuid();
    }
}
