using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Attendance.Commands;

public class ClockOutRequestValidator : CustomValidator<ClockOutRequest>
{
    public ClockOutRequestValidator(IStringLocalizer<ClockOutRequestValidator> T)
    {
        // EmployeeId can be null if derived from ICurrentUser.

        RuleFor(p => p.ClockOutTime)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)) // Allow for slight clock differences
                .WithMessage(T["Clock-out time cannot be significantly in the future."]);
        // Note: Comparison with ClockInTime will be done in the handler as it requires fetching the record.

        RuleFor(p => p.Notes)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.Notes));
    }
}
