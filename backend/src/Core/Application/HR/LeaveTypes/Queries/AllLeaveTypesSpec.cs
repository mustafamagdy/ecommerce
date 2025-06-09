using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For LeaveType entity

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class AllLeaveTypesSpec : Specification<LeaveType, LeaveTypeDto>
{
    public AllLeaveTypesSpec()
    {
        Query.OrderBy(lt => lt.Name); // Default ordering by Name

        // Projection to LeaveTypeDto
        Query.Select(lt => new LeaveTypeDto
        {
            Id = lt.Id,
            Name = lt.Name,
            DefaultBalance = lt.DefaultBalance,
            CreatedOn = lt.CreatedOn,
            LastModifiedOn = lt.LastModifiedOn,
            CreatedBy = lt.CreatedBy,
            LastModifiedBy = lt.LastModifiedBy
        });
    }
}
