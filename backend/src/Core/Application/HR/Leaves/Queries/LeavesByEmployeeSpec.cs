using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class LeavesByEmployeeSpec : Specification<Leave, LeaveDto>
{
    public LeavesByEmployeeSpec(Guid employeeId)
    {
        Query
            .Where(l => l.EmployeeId == employeeId)
            // .Include(l => l.Employee) // EmployeeFullName is already part of the DTO projection from LeaveByIdSpec logic
            .Include(l => l.LeaveType)   // To get LeaveType.Name
            .OrderByDescending(l => l.StartDate);

        Query.Select(l => new LeaveDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            // EmployeeFullName will be null here if Employee is not included, or requires another join/subquery.
            // For simplicity, assuming Employee details might be fetched separately if needed for a list view,
            // or the DTO is flexible. If EmployeeFullName is essential, .Include(l => l.Employee) is needed.
            // Let's include it for completeness as per DTO definition.
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

        // Re-including Employee for EmployeeFullName as DTO expects it.
        Query.Include(l => l.Employee);
    }
}
