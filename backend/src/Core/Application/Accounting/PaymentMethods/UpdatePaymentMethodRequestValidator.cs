using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class UpdatePaymentMethodRequestValidator : CustomValidator<UpdatePaymentMethodRequest>
{
    public UpdatePaymentMethodRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.Name is not null);

        RuleFor(p => p.Description)
            .MaximumLength(256)
            .When(p => p.Description is not null);
    }
}
