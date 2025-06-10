using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Applicant entity
using FSH.WebApi.Domain.HR.Enums; // For ApplicantStatus
using FSH.WebApi.Domain.HR.Recruitment.Events; // For ApplicantHiredEvent
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

public class UpdateApplicantStatusRequestHandler : IRequestHandler<UpdateApplicantStatusRequest, Guid>
{
    private readonly IRepositoryWithEvents<Applicant> _applicantRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateApplicantStatusRequestHandler(
        IRepositoryWithEvents<Applicant> applicantRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateApplicantStatusRequestHandler> localizer)
    {
        _applicantRepo = applicantRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateApplicantStatusRequest request, CancellationToken cancellationToken)
    {
        var applicant = await _applicantRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = applicant ?? throw new NotFoundException(_t["Applicant with ID {0} not found.", request.Id]);

        if (applicant.Status == request.Status)
        {
            // If notes are provided, update notes even if status is same.
            if (request.Notes is not null && applicant.Notes != request.Notes)
            {
                applicant.Notes = request.Notes; // Or append to existing notes
            }
            else if (request.Notes is null && applicant.Notes is not null) // Clear notes if request.Notes is null
            {
                applicant.Notes = null;
            }
            else
            {
                 return applicant.Id; // No actual change if notes also not provided or same
            }
        }
        else
        {
            applicant.Status = request.Status;
            // Update notes if provided, or clear if explicitly null
            if (request.Notes is not null || applicant.Notes is not null) // Check if notes changed or one of them is not null
            {
                 applicant.Notes = request.Notes; // Or append: $"{applicant.Notes}\nStatus changed to {request.Status}: {request.Notes}"
            }
        }

        // Add a general update event
        applicant.AddDomainEvent(EntityUpdatedEvent.WithEntity(applicant));

        // If applicant is hired, add the specific ApplicantHiredEvent
        if (applicant.Status == ApplicantStatus.Hired)
        {
            var applicantHiredEvent = new ApplicantHiredEvent(applicant.Id, applicant.JobOpeningId, DateTime.UtcNow);
            applicant.AddDomainEvent(applicantHiredEvent);
        }

        await _applicantRepo.UpdateAsync(applicant, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return applicant.Id;
    }
}
