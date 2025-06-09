using FSH.WebApi.Application.Common.Validation;
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
            .NotEmpty()
            .Must(status => status == "Approved" || status == "Rejected")
                .WithMessage(T["Status must be either 'Approved' or 'Rejected'."]);

        RuleFor(p => p.Notes)
            .MaximumLength(500)
                .WithMessage(T["Notes must not exceed 500 characters."])
            .When(p => p.Notes is not null);
    }
}
