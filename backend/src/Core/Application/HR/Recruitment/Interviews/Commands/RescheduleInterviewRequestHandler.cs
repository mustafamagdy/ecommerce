using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Domain.HR.Enums;
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RescheduleInterviewRequestHandler : IRequestHandler<RescheduleInterviewRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IReadRepository<Employee> _employeeRepo; // To validate NewInterviewerId
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public RescheduleInterviewRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IReadRepository<Employee> employeeRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<RescheduleInterviewRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _employeeRepo = employeeRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(RescheduleInterviewRequest request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepo.GetByIdAsync(request.InterviewId, cancellationToken);
        _ = interview ?? throw new NotFoundException(_t["Interview with ID {0} not found.", request.InterviewId]);

        if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.Rescheduled) // Allow rescheduling if already rescheduled
        {
            throw new ConflictException(_t["Only scheduled or previously rescheduled interviews can be rescheduled. Current status: {0}", interview.Status]);
        }

        // Update fields
        bool timeOrInterviewerChanged = false;
        var originalNotes = interview.Notes ?? string.Empty;

        if (request.NewScheduledTime != DateTime.MinValue && request.NewScheduledTime != interview.ScheduledTime)
        {
            interview.ScheduledTime = request.NewScheduledTime;
            timeOrInterviewerChanged = true;
        }

        Guid interviewerForCheck = interview.InterviewerId;
        if (request.NewInterviewerId.HasValue && request.NewInterviewerId.Value != interview.InterviewerId)
        {
            var newInterviewer = await _employeeRepo.GetByIdAsync(request.NewInterviewerId.Value, cancellationToken);
            _ = newInterviewer ?? throw new NotFoundException(_t["New Interviewer (Employee) with ID {0} not found.", request.NewInterviewerId.Value]);
            interview.InterviewerId = request.NewInterviewerId.Value;
            interviewerForCheck = request.NewInterviewerId.Value;
            timeOrInterviewerChanged = true;
        }

        if (request.NewLocation is not null && request.NewLocation != interview.Location)
        {
            interview.Location = request.NewLocation;
        }
        if (request.NewInterviewType.HasValue && request.NewInterviewType.Value != interview.Type)
        {
            interview.Type = request.NewInterviewType.Value;
        }


        if (timeOrInterviewerChanged)
        {
            var proposedStartTime = interview.ScheduledTime;
            var proposedEndTime = interview.ScheduledTime.AddHours(1); // Approximate for conflict check

            var conflictSpec = new InterviewsByInterviewerAtTimeSpec(interviewerForCheck, proposedStartTime, proposedEndTime, interview.Id); // Exclude current interview
            var conflictingInterview = await _interviewRepo.FirstOrDefaultAsync(conflictSpec, cancellationToken);
            if (conflictingInterview is not null)
            {
                var interviewerName = interview.Interviewer?.FirstName + " " + interview.Interviewer?.LastName ?? interviewerForCheck.ToString(); // Requires .Include(i=>i.Interviewer) on GetByIdAsync or fetch here
                throw new ConflictException(_t["The new time/interviewer conflicts with an existing engagement for interviewer {0} at {1}.", interviewerName, interview.ScheduledTime]);
            }
        }

        string newNotes = originalNotes;
        if (!string.IsNullOrWhiteSpace(request.RescheduleReason))
        {
            newNotes = string.IsNullOrWhiteSpace(originalNotes)
                ? $"Rescheduled: {request.RescheduleReason}"
                : $"{originalNotes}\nRescheduled: {request.RescheduleReason}";
        }
        interview.Notes = newNotes;
        interview.Status = InterviewStatus.Rescheduled; // Set to Rescheduled

        interview.AddDomainEvent(EntityUpdatedEvent.WithEntity(interview));
        await _interviewRepo.UpdateAsync(interview, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return interview.Id;
    }
}
