using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;       // For Interview, Applicant, Employee entities
using FSH.WebApi.Domain.HR.Enums; // For ApplicantStatus, InterviewStatus enums
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications; // For InterviewsByInterviewerAtTimeSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class ScheduleInterviewRequestHandler : IRequestHandler<ScheduleInterviewRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IRepositoryWithEvents<Applicant> _applicantRepo; // Changed to IRepositoryWithEvents for status update
    private readonly IReadRepository<Employee> _employeeRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public ScheduleInterviewRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IRepositoryWithEvents<Applicant> applicantRepo, // Changed
        IReadRepository<Employee> employeeRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<ScheduleInterviewRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _applicantRepo = applicantRepo; // Changed
        _employeeRepo = employeeRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(ScheduleInterviewRequest request, CancellationToken cancellationToken)
    {
        var applicant = await _applicantRepo.GetByIdAsync(request.ApplicantId, cancellationToken);
        _ = applicant ?? throw new NotFoundException(_t["Applicant with ID {0} not found.", request.ApplicantId]);

        // Validate Applicant status
        if (applicant.Status != ApplicantStatus.Shortlisted && applicant.Status != ApplicantStatus.Interviewing && applicant.Status != ApplicantStatus.InterviewScheduled)
        {
            throw new ConflictException(_t["Applicant is not in a state where an interview can be scheduled. Current status: {0}", applicant.Status]);
        }

        var interviewer = await _employeeRepo.GetByIdAsync(request.InterviewerId, cancellationToken);
        _ = interviewer ?? throw new NotFoundException(_t["Interviewer (Employee) with ID {0} not found.", request.InterviewerId]);

        // Basic interviewer availability check (e.g., +/- 1 hour around the scheduled time)
        // Assuming interview duration is roughly 1 hour for this check. A more robust solution would use interview end times.
        var proposedStartTime = request.ScheduledTime;
        var proposedEndTime = request.ScheduledTime.AddHours(1); // Approximate end time for conflict check

        var conflictSpec = new InterviewsByInterviewerAtTimeSpec(request.InterviewerId, proposedStartTime, proposedEndTime);
        var conflictingInterview = await _interviewRepo.FirstOrDefaultAsync(conflictSpec, cancellationToken);
        if (conflictingInterview is not null)
        {
            throw new ConflictException(_t["Interviewer {0} already has an engagement around the proposed time {1}.", interviewer.FirstName + " " + interviewer.LastName, request.ScheduledTime]);
        }

        var interview = new Interview
        {
            ApplicantId = request.ApplicantId,
            InterviewerId = request.InterviewerId,
            ScheduledTime = request.ScheduledTime,
            Type = request.Type,
            Location = request.Location, // Added Location
            Notes = request.Notes,
            Status = InterviewStatus.Scheduled
        };

        // Optionally update Applicant status
        if (applicant.Status != ApplicantStatus.InterviewScheduled && applicant.Status != ApplicantStatus.Interviewing)
        {
            // This logic might be more complex e.g. if multiple interviews, status becomes "Interviewing"
            applicant.Status = ApplicantStatus.InterviewScheduled;
            applicant.AddDomainEvent(EntityUpdatedEvent.WithEntity(applicant)); // Ensure applicant update event is raised
            await _applicantRepo.UpdateAsync(applicant, cancellationToken);
        }


        interview.AddDomainEvent(EntityCreatedEvent.WithEntity(interview));
        await _interviewRepo.AddAsync(interview, cancellationToken);

        await _uow.CommitAsync(cancellationToken); // Single commit for all changes

        return interview.Id;
    }
}
