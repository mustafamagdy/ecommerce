using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For LeaveType
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveRequestValidator : CustomValidator<UpdateLeaveRequest>
{
    public UpdateLeaveRequestValidator(
        IReadRepository<LeaveType> leaveTypeRepository, // Only if LeaveTypeId can be updated
        IStringLocalizer<UpdateLeaveRequestValidator> T)
    {
        RuleFor(p => p.Id)
            .NotEmpty();

        RuleFor(p => p.LeaveTypeId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await leaveTypeRepository.FirstOrDefaultAsync(new LeaveTypeExistsSpec(id!.Value), ct) is not null)
                .WithMessage(T["LeaveType with ID {0} not found.", (req, id) => id])
            .When(p => p.LeaveTypeId.HasValue);

        RuleFor(p => p.StartDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage(T["Start date cannot be in the past."])
            .When(p => p.StartDate.HasValue);

        When(p => p.EndDate.HasValue, () => {
            RuleFor(p => p.EndDate!.Value) // Use EndDate.Value because of When condition
                .NotEmpty().WithMessage(T["End date cannot be empty when provided."])
                .GreaterThanOrEqualTo(p => p.StartDate!.Value)
                    .WithMessage(T["End date must be on or after the start date."])
                .When(p => p.StartDate.HasValue); // This inner When is important if StartDate is optional
        });

        RuleFor(p => p.Reason)
            .MaximumLength(500)
                .WithMessage(T["Reason must not exceed 500 characters."])
            .When(p => p.Reason is not null);
    }
}
