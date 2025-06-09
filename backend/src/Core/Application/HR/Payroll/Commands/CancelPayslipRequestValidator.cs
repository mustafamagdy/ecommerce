using FSH.WebApi.Application.Common.Validation;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class CancelPayslipRequestValidator : CustomValidator<CancelPayslipRequest>
{
    public CancelPayslipRequestValidator(IStringLocalizer<CancelPayslipRequestValidator> T)
    {
        RuleFor(p => p.PayslipId)
            .NotEmpty();

        RuleFor(p => p.Reason)
            .MaximumLength(500).WithMessage(T["Cancellation reason must not exceed 500 characters."])
            .When(p => !string.IsNullOrEmpty(p.Reason));
    }
}
