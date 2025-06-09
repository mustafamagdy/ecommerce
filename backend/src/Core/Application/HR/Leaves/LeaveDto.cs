using FSH.WebApi.Domain.HR.Enums; // For LeaveStatusEnum

namespace FSH.WebApi.Application.HR.Leaves;

public class LeaveDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public string? EmployeeFullName { get; set; } // Combination of FirstName and LastName

    public Guid LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveStatusEnum Status { get; set; } // Changed to Enum
    public string StatusDescription => Status.ToString(); // String representation

    public string? Notes { get; set; } // Manager's comments

    public DateTime RequestedDate { get; set; } // Mapped from CreatedOn
    public DateTime? ActionDate { get; set; } // Mapped from LastModifiedOn (when status changes from Pending)
}
