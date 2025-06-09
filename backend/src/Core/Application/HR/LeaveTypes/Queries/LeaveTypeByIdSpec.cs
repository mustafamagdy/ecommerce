using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For LeaveType entity

namespace FSH.WebApi.Application.HR.LeaveTypes.Queries;

public class LeaveTypeByIdSpec : Specification<LeaveType, LeaveTypeDto>, ISingleResultSpecification
{
    public LeaveTypeByIdSpec(Guid leaveTypeId)
    {
        Query
            .Where(lt => lt.Id == leaveTypeId);

        // Projection to LeaveTypeDto
        // AuditableEntity properties (CreatedOn, LastModifiedOn etc.)
        // should be mapped automatically if the names match.
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
