using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

namespace FSH.WebApi.Domain.HR;

public enum PayslipStatus
{
    Draft,
    Generated,
    Paid,
    Cancelled // Added for completeness
}

public class Payslip : AuditableEntity
{
    public Guid EmployeeId { get; set; } // Foreign Key to Employee
    public virtual Employee? Employee { get; set; } // Navigation property

    public DateTime PayPeriodStartDate { get; set; }
    public DateTime PayPeriodEndDate { get; set; }

    public decimal BasicSalaryPaid { get; set; } // Basic salary component for this period
    public decimal TotalEarnings { get; set; }   // Sum of all earning components for this period
    public decimal TotalDeductions { get; set; } // Sum of all deduction components for this period
    public decimal NetSalary { get; set; }       // BasicSalaryPaid + TotalEarnings - TotalDeductions

    public DateTime GeneratedDate { get; set; }
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;
    public string? CancellationReason { get; set; } // Added for cancellation reason

    public List<PayslipComponent> Components { get; set; } = new(); // Snapshot of components

    // Default constructor
    public Payslip()
    {
        GeneratedDate = DateTime.UtcNow;
    }
}
