using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.LeaveTypes.Commands;

public class UpdateLeaveTypeRequestValidator : CustomValidator<UpdateLeaveTypeRequest>
{
    public UpdateLeaveTypeRequestValidator(IStringLocalizer<UpdateLeaveTypeRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100)
                .WithMessage(T["Name must not exceed 100 characters."])
            .When(p => p.Name is not null); // Only validate if Name is provided for update

        RuleFor(p => p.DefaultBalance)
            .GreaterThanOrEqualTo(0)
                .WithMessage(T["Default Balance must be greater than or equal to 0."])
            .When(p => p.DefaultBalance.HasValue); // Only validate if DefaultBalance is provided
    }
}
