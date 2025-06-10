using FluentValidation;
using FSH.WebApi.Application.Common.Validation;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class CreatePaymentMethodRequestValidator : CustomValidator<CreatePaymentMethodRequest>
{
    public CreatePaymentMethodRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .MaximumLength(256);
    }
}
