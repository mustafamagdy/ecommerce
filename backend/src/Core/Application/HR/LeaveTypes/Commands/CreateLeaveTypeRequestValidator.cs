using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class CreateLeaveTypeRequestValidator : CustomValidator<CreateLeaveTypeRequest>
{
    public CreateLeaveTypeRequestValidator(IStringLocalizer<CreateLeaveTypeRequestValidator> T)
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100)
                .WithMessage(T["Name must not exceed 100 characters."]);

        RuleFor(p => p.DefaultBalance)
            .GreaterThanOrEqualTo(0)
                .WithMessage(T["Default Balance must be greater than or equal to 0."]);
    }
}
