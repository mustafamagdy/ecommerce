using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Leaves.Specifications;

public class EmployeeLeaveBalanceSpec : Specification<EmployeeLeaveBalance>, ISingleResultSpecification
{
    public EmployeeLeaveBalanceSpec(Guid employeeId, Guid leaveTypeId)
    {
        Query.Where(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId);
    }
}
