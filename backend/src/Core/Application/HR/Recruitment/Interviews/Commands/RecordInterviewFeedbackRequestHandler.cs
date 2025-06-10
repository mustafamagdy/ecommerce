using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;       // For Interview, Applicant entities
using FSH.WebApi.Domain.HR.Enums; // For InterviewStatus, ApplicantStatus enums
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RecordInterviewFeedbackRequestHandler : IRequestHandler<RecordInterviewFeedbackRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IRepositoryWithEvents<Applicant> _applicantRepo; // Added for updating Applicant status
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;
    // Potentially ICurrentUser to check if the user is the assigned interviewer or a recruiter

    public RecordInterviewFeedbackRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IRepositoryWithEvents<Applicant> applicantRepo, // Added
        IApplicationUnitOfWork uow,
        IStringLocalizer<RecordInterviewFeedbackRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _applicantRepo = applicantRepo; // Added
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(RecordInterviewFeedbackRequest request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = interview ?? throw new NotFoundException(_t["Interview with ID {0} not found.", request.Id]);

        // Optional: Check if current user is authorized to submit feedback (e.g., is the InterviewerId or a Recruiter)
        // Optional: Check if interview is in a state where feedback can be submitted (e.g., Scheduled or HappeningNow)
        // if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.InProgress) // Assuming InProgress status
        // {
        //    throw new ConflictException(_t["Feedback can only be recorded for scheduled or in-progress interviews."]);
        // }

        interview.Feedback = request.Feedback;
        interview.Status = request.InterviewNewStatus; // Use InterviewNewStatus from request

        interview.AddDomainEvent(EntityUpdatedEvent.WithEntity(interview));
        await _interviewRepo.UpdateAsync(interview, cancellationToken);

        // Optional: Update Applicant Status based on NextRecommendedApplicantStep
        if (request.NextRecommendedApplicantStep.HasValue)
        {
            var applicant = await _applicantRepo.GetByIdAsync(interview.ApplicantId, cancellationToken);
            if (applicant is not null)
            {
                applicant.Status = request.NextRecommendedApplicantStep.Value;
                // Potentially add a note to applicant about this status change origin
                // applicant.Notes = $"{applicant.Notes}\nStatus updated to {applicant.Status} after interview {interview.Id} feedback.";
                applicant.AddDomainEvent(EntityUpdatedEvent.WithEntity(applicant));
                await _applicantRepo.UpdateAsync(applicant, cancellationToken);
            }
            else
            {
                // Log or handle missing applicant - though FK constraint should prevent this state.
                _t.LogWarning("Applicant with ID {ApplicantId} not found when trying to update status after interview {InterviewId} feedback.", interview.ApplicantId, interview.Id);
            }
        }

        await _uow.CommitAsync(cancellationToken); // Single commit for all changes

        return interview.Id;
    }
}
