using FSH.WebApi.Application.Common.Validation;
using FluentValidation;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CancelLeaveRequestValidator : CustomValidator<CancelLeaveRequest>
{
    public CancelLeaveRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty();
    }
}
