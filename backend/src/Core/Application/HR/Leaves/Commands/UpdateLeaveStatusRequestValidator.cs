using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveStatusRequestValidator : CustomValidator<UpdateLeaveStatusRequest>
{
    public UpdateLeaveStatusRequestValidator(IStringLocalizer<UpdateLeaveStatusRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum() // Validates it's a valid enum value
            .Must(status => status == LeaveStatusEnum.Approved || status == LeaveStatusEnum.Rejected) // Further restrict to only these two for this specific request
                .WithMessage(T["Status must be either 'Approved' or 'Rejected' for this operation."]);

        RuleFor(p => p.Notes)
            .MaximumLength(500)
                .WithMessage(T["Notes must not exceed 500 characters."])
            .When(p => p.Notes is not null);
    }
}
