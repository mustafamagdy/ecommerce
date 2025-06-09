using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class PendingLeavesSpec : Specification<Leave, LeaveDto>
{
    public PendingLeavesSpec()
    {
        Query
            .Where(l => l.Status == "Pending") // Assuming "Pending" is the string representation.
                                               // Consider using an enum: LeaveStatus.Pending.ToString()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .OrderBy(l => l.CreatedOn); // Order by RequestedDate ascending

        Query.Select(l => new LeaveDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeFullName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : null,
            LeaveTypeId = l.LeaveTypeId,
            LeaveTypeName = l.LeaveType != null ? l.LeaveType.Name : null,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            Status = l.Status,
            Notes = l.Notes,
            RequestedDate = l.CreatedOn,
            ActionDate = l.LastModifiedOn
        });
    }
}
