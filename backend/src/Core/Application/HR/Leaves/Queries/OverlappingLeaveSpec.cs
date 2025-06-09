using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Leave entity

namespace FSH.WebApi.Application.HR.Leaves.Queries;

public class OverlappingLeaveSpec : Specification<Leave>, ISingleResultSpecification
{
    public OverlappingLeaveSpec(Guid employeeId, DateTime startDate, DateTime endDate, Guid? leaveIdToExclude = null)
    {
        Query
            .Where(l => l.EmployeeId == employeeId)
            .Where(l => l.StartDate < endDate && l.EndDate > startDate) // Checks for overlap
            .Where(l => l.Status != "Rejected" && l.Status != "Cancelled"); // Only consider active or pending leaves
                                                                            // Consider using enum: l.Status != LeaveStatus.Rejected.ToString() ...

        if (leaveIdToExclude.HasValue)
        {
            Query.Where(l => l.Id != leaveIdToExclude.Value);
        }
    }
}
