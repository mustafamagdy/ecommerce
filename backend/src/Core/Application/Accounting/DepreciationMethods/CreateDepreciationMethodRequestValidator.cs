using FluentValidation;
using FSH.WebApi.Application.Common.Validation;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class CreateDepreciationMethodRequestValidator : CustomValidator<CreateDepreciationMethodRequest>
{
    public CreateDepreciationMethodRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .MaximumLength(256);
    }
}
