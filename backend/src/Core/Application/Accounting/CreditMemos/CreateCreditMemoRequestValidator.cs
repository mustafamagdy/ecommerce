using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class CreateCreditMemoRequestValidator : CustomValidator<CreateCreditMemoRequest>
{
    public CreateCreditMemoRequestValidator()
    {
        RuleFor(p => p.CustomerId)
            .NotEmptyGuid();

        RuleFor(p => p.Date)
            .NotEmpty();

        RuleFor(p => p.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(p => p.TotalAmount)
            .GreaterThan(0);

        RuleFor(p => p.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(p => p.Notes)
            .MaximumLength(2000);

        RuleFor(p => p.OriginalCustomerInvoiceId)
            .NotEmptyGuid()
            .When(p => p.OriginalCustomerInvoiceId.HasValue);
    }
}
