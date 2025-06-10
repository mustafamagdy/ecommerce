using FluentValidation;
using FSH.WebApi.Application.Common.Validation;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class CreatePaymentTermRequestValidator : CustomValidator<CreatePaymentTermRequest>
{
    public CreatePaymentTermRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .MaximumLength(256);

        RuleFor(p => p.DaysUntilDue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Days until due must be zero or positive.");
        // IsActive is a bool, typically doesn't need specific validation unless to prevent certain values.
    }
}
