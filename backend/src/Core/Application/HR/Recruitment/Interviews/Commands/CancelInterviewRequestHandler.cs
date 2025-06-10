using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Domain.HR.Enums;
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class CancelInterviewRequestHandler : IRequestHandler<CancelInterviewRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public CancelInterviewRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<CancelInterviewRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(CancelInterviewRequest request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepo.GetByIdAsync(request.InterviewId, cancellationToken);
        _ = interview ?? throw new NotFoundException(_t["Interview with ID {0} not found.", request.InterviewId]);

        // Allow cancellation if it's Scheduled or Rescheduled. Completed interviews cannot be simply cancelled.
        if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.Rescheduled)
        {
            throw new ConflictException(_t["Only scheduled or rescheduled interviews can be cancelled. Current status: {0}", interview.Status]);
        }

        // Determine which cancelled status to use. Assuming CancelledByInterviewer if not specified.
        // A more complex system might take a parameter for who initiated the cancellation.
        interview.Status = InterviewStatus.CancelledByInterviewer; // Or a generic "Cancelled" if added to enum

        string newNotes = interview.Notes ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(request.CancellationReason))
        {
            newNotes = string.IsNullOrWhiteSpace(newNotes)
                ? $"Cancelled: {request.CancellationReason}"
                : $"{newNotes}\nCancelled: {request.CancellationReason}";
        }
        interview.Notes = newNotes;


        interview.AddDomainEvent(EntityUpdatedEvent.WithEntity(interview));
        await _interviewRepo.UpdateAsync(interview, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return interview.Id;
    }
}
