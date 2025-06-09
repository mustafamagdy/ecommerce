using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Interview entity
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RecordInterviewFeedbackRequestHandler : IRequestHandler<RecordInterviewFeedbackRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;
    // Potentially ICurrentUser to check if the user is the assigned interviewer or a recruiter

    public RecordInterviewFeedbackRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<RecordInterviewFeedbackRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
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
        interview.Status = request.NewStatus; // Typically set to Completed

        interview.AddDomainEvent(EntityUpdatedEvent.WithEntity(interview));
        await _interviewRepo.UpdateAsync(interview, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return interview.Id;
    }
}
