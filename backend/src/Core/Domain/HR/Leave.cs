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
    public string Status { get; set; } = string.Empty; // e.g., Pending, Approved, Rejected
}
