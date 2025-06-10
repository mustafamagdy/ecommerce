using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces; // For ICurrentUser
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum
using FSH.WebApi.Application.HR.Leaves.Specifications; // For EmployeeLeaveBalanceSpec
using MediatR;
using FluentValidation.Results; // For ValidationFailure for ValidationException
using System.Collections.Generic; // For List
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Leaves.Commands;

public class UpdateLeaveStatusRequestHandler : IRequestHandler<UpdateLeaveStatusRequest, Guid>
{
    private readonly IRepositoryWithEvents<Leave> _leaveRepository;
    private readonly IRepository<EmployeeLeaveBalance> _leaveBalanceRepository; // Changed to IRepository for update
    private readonly ICurrentUser _currentUser; // Uncommented for manager check
    private readonly IReadRepository<Employee> _employeeRepository; // For manager check
    private readonly IStringLocalizer _t;
    private readonly IApplicationUnitOfWork _uow;

    public UpdateLeaveStatusRequestHandler(
        IRepositoryWithEvents<Leave> leaveRepository,
        IRepository<EmployeeLeaveBalance> leaveBalanceRepository, // Changed
        ICurrentUser currentUser, // Uncommented
        IReadRepository<Employee> employeeRepository, // Added
        IStringLocalizer<UpdateLeaveStatusRequestHandler> localizer,
        IApplicationUnitOfWork uow)
    {
        _leaveRepository = leaveRepository;
        _leaveBalanceRepository = leaveBalanceRepository; // Changed
        _currentUser = currentUser; // Uncommented
        _employeeRepository = employeeRepository; // Added
        _t = localizer;
        _uow = uow;
    }

    public async Task<Guid> Handle(UpdateLeaveStatusRequest request, CancellationToken cancellationToken)
    {
        var leave = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = leave ?? throw new NotFoundException(_t["Leave request with ID {0} not found.", request.Id]);

        // Manager Authorization (as per subtask point 2)
        var employee = await _employeeRepository.GetByIdAsync(leave.EmployeeId, cancellationToken);
        _ = employee ?? throw new NotFoundException(_t["Employee data for this leave request not found."]); // Should not happen if FK is good

        Guid? managerUserId = _currentUser.GetUserId(); // Assuming GetUserId() returns the Employee ID of the manager
        if (employee.ManagerId != managerUserId)
        {
            throw new ForbiddenException(_t["You are not authorized to approve/reject this leave request."]);
        }

        if (leave.Status != LeaveStatusEnum.Pending) // Using Enum
        {
            throw new ConflictException(_t["Only pending leave requests can be approved or rejected. This leave is already {0}.", leave.Status]);
        }

        // Request status validation is handled by FluentValidation (IsInEnum)

        leave.Status = request.Status; // Using Enum from request
        leave.Notes = request.Notes ?? leave.Notes;

        if (request.Status == LeaveStatusEnum.Approved)
        {
            decimal requestedDuration = (decimal)(leave.EndDate - leave.StartDate).TotalDays + 1;
            var balanceSpec = new EmployeeLeaveBalanceSpec(leave.EmployeeId, leave.LeaveTypeId);
            var balance = await _leaveBalanceRepository.FirstOrDefaultAsync(balanceSpec, cancellationToken);

            if (balance is null)
            {
                // This case means the leave was created without a balance record (perhaps allowed by policy or a new leave type)
                // Depending on strictness, could throw error or create a negative balance.
                // For now, we'll note that this needs a policy decision.
                // To prevent issues, we could throw a specific error if balance deduction is always mandatory.
                // For example: throw new ConflictException(_t["Cannot approve: Employee leave balance record not found for this leave type."]);
                // Or, if a negative balance is allowed or initial balance is assumed from LeaveType.DefaultBalance:
                // balance = new EmployeeLeaveBalance(leave.EmployeeId, leave.LeaveTypeId, 0); // Assume 0 if not found, leads to negative
                // await _leaveBalanceRepository.AddAsync(balance, cancellationToken);
                // For this subtask, "perform deduction if record exists"
                 throw new ConflictException(_t["Leave balance record not found for employee {0} and leave type {1}. Cannot approve.", leave.EmployeeId, leave.LeaveTypeId]);
            }
            else if (balance.BalanceDays < requestedDuration)
            {
                // This check should ideally also be in CreateLeave, but good to double check or if balance changed.
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure("Balance", _t["Insufficient leave balance at time of approval. Available: {0} days, Requested: {1} days.", balance.BalanceDays, requestedDuration])
                };
                throw new ValidationException(failures); // Or ConflictException
            }
            else
            {
                balance.BalanceDays -= requestedDuration;
                await _leaveBalanceRepository.UpdateAsync(balance, cancellationToken);
            }
        }

        // Domain Event for status change could be added here if needed for notifications etc.
        // leave.AddDomainEvent(new LeaveStatusUpdatedEvent(leave));
        await _leaveRepository.UpdateAsync(leave, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return leave.Id;
    }
}
