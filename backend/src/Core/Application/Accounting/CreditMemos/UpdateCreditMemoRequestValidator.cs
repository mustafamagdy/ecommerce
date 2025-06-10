using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class UpdateCreditMemoRequestValidator : CustomValidator<UpdateCreditMemoRequest>
{
    public UpdateCreditMemoRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.Date)
            .NotEmpty()
            .When(p => p.Date.HasValue);

        RuleFor(p => p.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .When(p => p.Reason is not null);

        RuleFor(p => p.TotalAmount)
            .GreaterThan(0)
            .When(p => p.TotalAmount.HasValue);

        RuleFor(p => p.Currency)
            .Length(3)
            .When(p => !string.IsNullOrEmpty(p.Currency));

        RuleFor(p => p.Notes)
            .MaximumLength(2000)
            .When(p => p.Notes is not null);

        RuleFor(p => p.OriginalCustomerInvoiceId)
            .NotEmptyGuid()
            .When(p => p.OriginalCustomerInvoiceId.HasValue);
    }
}
