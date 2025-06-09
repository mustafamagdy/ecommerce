using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Interview entity
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class UpdateInterviewStatusRequestHandler : IRequestHandler<UpdateInterviewStatusRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateInterviewStatusRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateInterviewStatusRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateInterviewStatusRequest request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = interview ?? throw new NotFoundException(_t["Interview with ID {0} not found.", request.Id]);

        if (interview.Status == request.Status)
        {
            // If notes are provided, update notes even if status is same.
            if (request.Notes is not null && interview.Notes != request.Notes) // Check if notes actually changed
            {
                interview.Notes = request.Notes; // Or append to existing notes
            }
            else
            {
                return interview.Id; // No actual status change or note update
            }
        }
        else
        {
            interview.Status = request.Status;
            if (request.Notes is not null)
            {
                // Consider how notes are handled: overwrite, append, or specific field for status change reason
                interview.Notes = request.Notes;
            }
        }

        // Example: If status changes to Rescheduled, ScheduledTime might need to be updated via a different request,
        // or this request could be expanded. For now, this only updates status and general notes.

        interview.AddDomainEvent(EntityUpdatedEvent.WithEntity(interview));
        await _interviewRepo.UpdateAsync(interview, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return interview.Id;
    }
}
