using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

namespace FSH.WebApi.Domain.HR;

public class EmployeeLeaveBalance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; } // Navigation

    public Guid LeaveTypeId { get; set; }
    public virtual LeaveType? LeaveType { get; set; } // Navigation

    public decimal BalanceDays { get; set; } // Using decimal for flexibility (e.g., half-day balances)

    // Parameterless constructor for EF Core
    public EmployeeLeaveBalance() {}

    public EmployeeLeaveBalance(Guid employeeId, Guid leaveTypeId, decimal balanceDays)
    {
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        BalanceDays = balanceDays;
    }
}
