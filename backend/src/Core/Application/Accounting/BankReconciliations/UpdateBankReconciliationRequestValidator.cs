using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using FSH.WebApi.Domain.Accounting; // For ReconciliationStatus

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class UpdateBankReconciliationRequestValidator : CustomValidator<UpdateBankReconciliationRequest>
{
    public UpdateBankReconciliationRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.ManuallyAssignedUnclearedChecks)
            .GreaterThanOrEqualTo(0)
            .When(p => p.ManuallyAssignedUnclearedChecks.HasValue);

        RuleFor(p => p.ManuallyAssignedDepositsInTransit)
            .GreaterThanOrEqualTo(0)
            .When(p => p.ManuallyAssignedDepositsInTransit.HasValue);

        RuleFor(p => p.Status)
            .IsInEnum()
            .When(p => p.Status.HasValue);
    }
}
