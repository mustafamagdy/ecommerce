using FSH.WebApi.Application.Common.Validation;
using FluentValidation;

namespace FSH.WebApi.Application.HR.Payroll.Commands;

public class MarkPayslipAsPaidRequestValidator : CustomValidator<MarkPayslipAsPaidRequest>
{
    public MarkPayslipAsPaidRequestValidator()
    {
        RuleFor(p => p.PayslipId)
            .NotEmpty();
    }
}
