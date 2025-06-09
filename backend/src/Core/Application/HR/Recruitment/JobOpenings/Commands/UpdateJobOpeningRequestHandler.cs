using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For JobOpening, Department entities
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningRequestHandler : IRequestHandler<UpdateJobOpeningRequest, Guid>
{
    private readonly IRepositoryWithEvents<JobOpening> _jobOpeningRepo;
    private readonly IReadRepository<Department> _departmentRepo; // To validate DepartmentId if changed
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateJobOpeningRequestHandler(
        IRepositoryWithEvents<JobOpening> jobOpeningRepo,
        IReadRepository<Department> departmentRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<UpdateJobOpeningRequestHandler> localizer)
    {
        _jobOpeningRepo = jobOpeningRepo;
        _departmentRepo = departmentRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateJobOpeningRequest request, CancellationToken cancellationToken)
    {
        var jobOpening = await _jobOpeningRepo.GetByIdAsync(request.Id, cancellationToken);
        _ = jobOpening ?? throw new NotFoundException(_t["Job Opening with ID {0} not found.", request.Id]);

        if (request.Title is not null && request.Title != jobOpening.Title)
        {
            // Optional: Check for title uniqueness if it's a business requirement
            // var titleSpec = new JobOpeningByTitleSpec(request.Title);
            // var existingByTitle = await _jobOpeningRepo.FirstOrDefaultAsync(titleSpec, cancellationToken);
            // if (existingByTitle is not null && existingByTitle.Id != jobOpening.Id)
            // {
            //    throw new ConflictException(_t["Job Opening with title '{0}' already exists.", request.Title]);
            // }
            jobOpening.Title = request.Title;
        }

        if (request.Description is not null)
        {
            jobOpening.Description = request.Description;
        }

        if (request.DepartmentId.HasValue && request.DepartmentId.Value != jobOpening.DepartmentId)
        {
            var department = await _departmentRepo.GetByIdAsync(request.DepartmentId.Value, cancellationToken);
            _ = department ?? throw new NotFoundException(_t["Department with ID {0} not found.", request.DepartmentId.Value]);
            jobOpening.DepartmentId = request.DepartmentId.Value;
        }

        if (request.Status.HasValue)
        {
            jobOpening.Status = request.Status.Value;
        }

        if (request.PostedDate.HasValue)
        {
            jobOpening.PostedDate = request.PostedDate.Value;
        }

        // Allow setting ClosingDate to null
        if (request.ClosingDate.HasValue || (request.ClosingDate is null && jobOpening.ClosingDate is not null))
        {
            jobOpening.ClosingDate = request.ClosingDate;
        }

        jobOpening.AddDomainEvent(EntityUpdatedEvent.WithEntity(jobOpening));
        await _jobOpeningRepo.UpdateAsync(jobOpening, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return jobOpening.Id;
    }
}
