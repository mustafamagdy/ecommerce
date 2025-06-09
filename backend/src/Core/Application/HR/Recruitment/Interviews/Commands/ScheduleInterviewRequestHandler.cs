using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Interview, Applicant, Employee entities
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class ScheduleInterviewRequestHandler : IRequestHandler<ScheduleInterviewRequest, Guid>
{
    private readonly IRepositoryWithEvents<Interview> _interviewRepo;
    private readonly IReadRepository<Applicant> _applicantRepo;
    private readonly IReadRepository<Employee> _employeeRepo; // Assuming Employee is the Interviewer
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public ScheduleInterviewRequestHandler(
        IRepositoryWithEvents<Interview> interviewRepo,
        IReadRepository<Applicant> applicantRepo,
        IReadRepository<Employee> employeeRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<ScheduleInterviewRequestHandler> localizer)
    {
        _interviewRepo = interviewRepo;
        _applicantRepo = applicantRepo;
        _employeeRepo = employeeRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(ScheduleInterviewRequest request, CancellationToken cancellationToken)
    {
        // Validate ApplicantId
        var applicant = await _applicantRepo.GetByIdAsync(request.ApplicantId, cancellationToken);
        _ = applicant ?? throw new NotFoundException(_t["Applicant with ID {0} not found.", request.ApplicantId]);

        // Validate InterviewerId (EmployeeId)
        var interviewer = await _employeeRepo.GetByIdAsync(request.InterviewerId, cancellationToken);
        _ = interviewer ?? throw new NotFoundException(_t["Interviewer (Employee) with ID {0} not found.", request.InterviewerId]);

        // Optional: Check for interviewer availability (complex, requires calendar integration or availability system)
        // Optional: Check if applicant is in a state where an interview can be scheduled (e.g., "Shortlisted", "Interviewing")

        var interview = new Interview
        {
            ApplicantId = request.ApplicantId,
            InterviewerId = request.InterviewerId,
            ScheduledTime = request.ScheduledTime,
            Type = request.Type,
            Notes = request.Notes, // Initial notes for the interview
            Status = InterviewStatus.Scheduled // Initial status
        };

        interview.AddDomainEvent(EntityCreatedEvent.WithEntity(interview));
        await _interviewRepo.AddAsync(interview, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return interview.Id;
    }
}
