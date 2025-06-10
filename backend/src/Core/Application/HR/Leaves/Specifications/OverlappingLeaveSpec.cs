using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Leave entity
using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum

namespace FSH.WebApi.Application.HR.Leaves.Specifications; // Corrected namespace

public class OverlappingLeaveSpec : Specification<Leave>, ISingleResultSpecification
{
    public OverlappingLeaveSpec(Guid employeeId, DateTime startDate, DateTime endDate, Guid? leaveIdToExclude = null)
    {
        Query
            .Where(l => l.EmployeeId == employeeId)
            .Where(l => l.StartDate < endDate && l.EndDate > startDate) // Checks for overlap
            .Where(l => l.Status != LeaveStatusEnum.Rejected && l.Status != LeaveStatusEnum.Cancelled); // Using Enum

        if (leaveIdToExclude.HasValue)
        {
            Query.Where(l => l.Id != leaveIdToExclude.Value);
        }
    }
}
