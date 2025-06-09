using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Leave, Employee, LeaveType entities

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class LeaveByIdSpec : Specification<Leave, LeaveDto>, ISingleResultSpecification
{
    public LeaveByIdSpec(Guid leaveId)
    {
        Query
            .Where(l => l.Id == leaveId)
            .Include(l => l.Employee)    // To get Employee.FirstName and Employee.LastName
            .Include(l => l.LeaveType);  // To get LeaveType.Name

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
            Notes = l.Notes, // Assuming 'Notes' is a direct property on Leave entity for manager's comments
            RequestedDate = l.CreatedOn, // Map from AuditableEntity.CreatedOn
            ActionDate = l.LastModifiedOn // Map from AuditableEntity.LastModifiedOn
        });
    }
}
