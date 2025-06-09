using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Domain.HR.Enums; // For JobOpeningStatus
using FluentValidation;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningStatusRequestValidator : CustomValidator<UpdateJobOpeningStatusRequest>
{
    public UpdateJobOpeningStatusRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.Status)
            .NotNull()
            .IsInEnum();
    }
}
