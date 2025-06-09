using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Leaves.Specifications; // For OverlappingLeaveSpec, EmployeeLeaveBalanceSpec
using MediatR;
using FluentValidation.Results; // For ValidationFailure, used by ValidationException
using System.Collections.Generic; // For List
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class CreateLeaveRequestHandler : IRequestHandler<CreateLeaveRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    // Employee and LeaveType repositories are likely used by validators now.
    // private readonly IReadRepository<Employee> _employeeRepository;
    // private readonly IReadRepository<LeaveType> _leaveTypeRepository;
    private readonly IReadRepository<EmployeeLeaveBalance> _leaveBalanceRepository; // Added
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public CreateLeaveRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        // IReadRepository<Employee> employeeRepository,
        // IReadRepository<LeaveType> leaveTypeRepository,
        IReadRepository<EmployeeLeaveBalance> leaveBalanceRepository, // Added
        IStringLocalizer<CreateLeaveRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        // _employeeRepository = employeeRepository;
        // _leaveTypeRepository = leaveTypeRepository;
        _leaveBalanceRepository = leaveBalanceRepository; // Added
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

        // Check leave balance
        decimal requestedDuration = (decimal)(request.EndDate - request.StartDate).TotalDays + 1;
        if (requestedDuration <= 0)
        {
            // This should ideally be caught by FluentValidation (EndDate >= StartDate)
            throw new ValidationException(new List<ValidationFailure> { new ValidationFailure(nameof(request.EndDate), _t["Leave duration must be at least one day."]) });
        }

        var balanceSpec = new EmployeeLeaveBalanceSpec(request.EmployeeId, request.LeaveTypeId);
        var balance = await _leaveBalanceRepository.FirstOrDefaultAsync(balanceSpec, cancellationToken);

        if (balance is null)
        {
            // Option: Allow if LeaveType has a very high default balance or is unlimited (not implemented here)
            // Option: Deny if no balance record exists (stricter)
            // For now, as per subtask: "if no record, assume not strictly tracked for this fix, focus on deduction."
            // This means we allow it here, but deduction on approval will fail if no record.
            // To be safer, one might throw a specific error or warning here.
            // E.g., throw new NotFoundException(_t["Leave balance record not found for this leave type."]);
        }
        else if (balance.BalanceDays < requestedDuration)
        {
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure(nameof(request.LeaveTypeId), _t["Insufficient leave balance. Available: {0} days, Requested: {1} days.", balance.BalanceDays, requestedDuration])
            };
            throw new ValidationException(failures);
        }

        var leave = new Leave
        {
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = FSH.WebApi.Domain.HR.Enums.LeaveStatusEnum.Pending, // Initial status using Enum
            // Notes will be empty initially
            // RequestedDate (CreatedOn) and ActionDate (LastModifiedOn) are set by AuditableEntity
        };

        leave.AddDomainEvent(EntityCreatedEvent.WithEntity(leave));

        await _leaveRepository.AddAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
