using System;
using System.Collections.Generic;

// Reusing the domain enum here, or could define a separate DTO enum
using FSH.WebApi.Domain.HR; // For PayslipStatus

namespace FSH.WebApi.Application.HR.Payroll;

public class PayslipDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; } // To be populated

    public DateTime PayPeriodStartDate { get; set; }
    public DateTime PayPeriodEndDate { get; set; }

    public decimal BasicSalaryPaid { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    public DateTime GeneratedDate { get; set; }
    public PayslipStatus Status { get; set; }

    public List<PayslipComponentDto> Components { get; set; } = new();

    public DateTime CreatedOn { get; set; } // From AuditableEntity
    public DateTime? LastModifiedOn { get; set; } // From AuditableEntity
}
