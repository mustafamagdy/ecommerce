using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.HR.Payroll;

public class SalaryStructureDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; } // To be populated (e.g., FirstName + LastName)

    public decimal BasicSalary { get; set; }
    public List<SalaryComponentDto> Earnings { get; set; } = new();
    public List<SalaryComponentDto> Deductions { get; set; } = new();

    // Optionally, include calculated totals if useful for the client
    public decimal CalculatedTotalEarnings { get; set; }
    public decimal CalculatedTotalDeductions { get; set; }
    public decimal CalculatedNetSalary { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
