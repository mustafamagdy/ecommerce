using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For JobOpening, Department entities
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class CreateJobOpeningRequestHandler : IRequestHandler<CreateJobOpeningRequest, Guid>
{
    private readonly IRepositoryWithEvents<JobOpening> _jobOpeningRepo;
    private readonly IReadRepository<Department> _departmentRepo; // To validate DepartmentId
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public CreateJobOpeningRequestHandler(
        IRepositoryWithEvents<JobOpening> jobOpeningRepo,
        IReadRepository<Department> departmentRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<CreateJobOpeningRequestHandler> localizer)
    {
        _jobOpeningRepo = jobOpeningRepo;
        _departmentRepo = departmentRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(CreateJobOpeningRequest request, CancellationToken cancellationToken)
    {
        // Validate DepartmentId
        var department = await _departmentRepo.GetByIdAsync(request.DepartmentId, cancellationToken);
        _ = department ?? throw new NotFoundException(_t["Department with ID {0} not found.", request.DepartmentId]);

        // Title uniqueness check (optional, based on requirements, can be done with a spec)
        // var titleSpec = new JobOpeningByTitleSpec(request.Title);
        // var existingByTitle = await _jobOpeningRepo.FirstOrDefaultAsync(titleSpec, cancellationToken);
        // if (existingByTitle is not null)
        // {
        //    throw new ConflictException(_t["Job Opening with title '{0}' already exists.", request.Title]);
        // }

        var jobOpening = new JobOpening
        {
            Title = request.Title,
            Description = request.Description,
            DepartmentId = request.DepartmentId,
            Status = request.Status,
            PostedDate = request.PostedDate,
            ClosingDate = request.ClosingDate
        };

        jobOpening.AddDomainEvent(EntityCreatedEvent.WithEntity(jobOpening));
        await _jobOpeningRepo.AddAsync(jobOpening, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return jobOpening.Id;
    }
}
