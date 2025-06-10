using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class ApplyCreditMemoToInvoiceRequestValidator : CustomValidator<ApplyCreditMemoToInvoiceRequest>
{
    public ApplyCreditMemoToInvoiceRequestValidator()
    {
        RuleFor(p => p.CreditMemoId)
            .NotEmptyGuid();

        RuleFor(p => p.CustomerInvoiceId)
            .NotEmptyGuid();

        RuleFor(p => p.AmountToApply)
            .GreaterThan(0);
    }
}
