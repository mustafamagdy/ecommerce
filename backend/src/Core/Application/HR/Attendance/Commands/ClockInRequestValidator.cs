using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockInRequestValidator : CustomValidator<ClockInRequest>
{
    public ClockInRequestValidator(IStringLocalizer<ClockInRequestValidator> T)
    {
        // EmployeeId can be null if derived from ICurrentUser, so no NotEmpty() here.
        // Handler will validate if EmployeeId is resolved.

        RuleFor(p => p.ClockInTime)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)) // Allow for slight clock differences, but not future clock-ins
                .WithMessage(T["Clock-in time cannot be in the future."]);

        RuleFor(p => p.Notes)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.Notes));
    }
}
