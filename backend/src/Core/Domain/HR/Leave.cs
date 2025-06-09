using FSH.WebApi.Domain.HR.Enums; // Added for LeaveStatusEnum

namespace FSH.WebApi.Domain.HR;

public class Leave : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }

    public Guid LeaveTypeId { get; set; }
    public virtual LeaveType? LeaveType { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveStatusEnum Status { get; set; } = LeaveStatusEnum.Pending; // Changed to Enum
    public string? Notes { get; set; } // Added from previous subtask for manager's comments
}
