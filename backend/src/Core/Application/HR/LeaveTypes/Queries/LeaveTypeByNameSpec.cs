using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For LeaveType entity

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class LeaveTypeByNameSpec : Specification<LeaveType>, ISingleResultSpecification
{
    public LeaveTypeByNameSpec(string name)
    {
        Query
            .Where(lt => lt.Name.ToLower() == name.ToLower()); // Case-insensitive comparison
    }
}
