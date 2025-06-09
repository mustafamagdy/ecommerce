using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Leaves.Queries; // For OverlappingLeaveSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CreateLeaveRequestHandler : IRequestHandler<CreateLeaveRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    private readonly IReadRepository<Employee> _employeeRepository; // To validate EmployeeId if not done by validator
    private readonly IReadRepository<LeaveType> _leaveTypeRepository; // To validate LeaveTypeId if not done by validator
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public CreateLeaveRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        IReadRepository<Employee> employeeRepository,
        IReadRepository<LeaveType> leaveTypeRepository,
        IStringLocalizer<CreateLeaveRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateLeaveRequest request, CancellationToken cancellationToken)
    {
        // FluentValidation should handle basic checks like existence of EmployeeId, LeaveTypeId.

        // Check for overlapping leaves
        var overlappingSpec = new OverlappingLeaveSpec(request.EmployeeId, request.StartDate, request.EndDate);
        var existingOverlappingLeave = await _leaveRepository.FirstOrDefaultAsync(overlappingSpec, cancellationToken);

        if (existingOverlappingLeave is not null)
        {
            throw new ConflictException(_t["An overlapping leave request already exists for this period."]);
        }

        // TODO: Check for sufficient leave balance (complex, follow-up enhancement)
        // This would involve:
        // 1. Getting LeaveType.DefaultBalance or Employee specific balance.
        // 2. Calculating total days requested.
        // 3. Calculating total approved/taken leave days for the period/year.
        // 4. Ensuring (Balance - Taken - Requested) >= 0.

        var leave = new Leave
        {
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = "Pending" // Initial status
            // Notes will be empty initially
            // RequestedDate (CreatedOn) and ActionDate (LastModifiedOn) are set by AuditableEntity
        };

        leave.AddDomainEvent(EntityCreatedEvent.WithEntity(leave));

        await _leaveRepository.AddAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
