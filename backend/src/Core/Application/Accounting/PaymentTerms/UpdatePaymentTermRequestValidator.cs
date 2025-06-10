using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class UpdatePaymentTermRequestValidator : CustomValidator<UpdatePaymentTermRequest>
{
    public UpdatePaymentTermRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.Name is not null); // Only validate if Name is provided for update

        RuleFor(p => p.Description)
            .MaximumLength(256)
            .When(p => p.Description is not null);

        RuleFor(p => p.DaysUntilDue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Days until due must be zero or positive.")
            .When(p => p.DaysUntilDue.HasValue);
    }
}
