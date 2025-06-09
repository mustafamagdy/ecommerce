using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For JobOpening entity
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningStatusRequestHandler : IRequestHandler<UpdateJobOpeningStatusRequest, Guid>
{
    private readonly IRepositoryWithEvents<JobOpening> _jobOpeningRepo;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateJobOpeningStatusRequestHandler(
        IRepositoryWithEvents<JobOpening> jobOpeningRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateJobOpeningStatusRequestHandler> localizer)
    {
        _jobOpeningRepo = jobOpeningRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateJobOpeningStatusRequest request, CancellationToken cancellationToken)
    {
        var jobOpening = await _jobOpeningRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = jobOpening ?? throw new NotFoundException(_t["Job Opening with ID {0} not found.", request.Id]);

        if (jobOpening.Status == request.Status)
        {
            // No change, could return early or throw specific exception if needed
            return jobOpening.Id;
        }

        var oldStatus = jobOpening.Status;
        jobOpening.Status = request.Status;

        // If closing a job, might set ClosingDate if not already set.
        if (request.Status == JobOpeningStatus.Closed && jobOpening.ClosingDate is null)
        {
            jobOpening.ClosingDate = DateTime.UtcNow;
        }
        // If re-opening a job (from Closed or OnHold), clear the ClosingDate so it can be set anew or be open-ended.
        else if ((oldStatus == JobOpeningStatus.Closed || oldStatus == JobOpeningStatus.OnHold) && request.Status == JobOpeningStatus.Open)
        {
            jobOpening.ClosingDate = null;
        }

        jobOpening.AddDomainEvent(EntityUpdatedEvent.WithEntity(jobOpening));
        await _jobOpeningRepo.UpdateAsync(jobOpening, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return jobOpening.Id;
    }
}
