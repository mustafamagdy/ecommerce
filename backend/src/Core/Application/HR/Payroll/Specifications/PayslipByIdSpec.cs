using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Payslip, Employee, PayslipComponent entities
using System.Linq;

namespace FSH.WebApi.Application.HR.Payroll.Specifications;

public class PayslipByIdSpec : Specification<Payslip, PayslipDto>, ISingleResultSpecification
{
    public PayslipByIdSpec(Guid payslipId)
    {
        Query
            .Where(p => p.Id == payslipId)
            .Include(p => p.Employee); // To get Employee.FirstName and Employee.LastName
            // Payslip.Components are owned entities, usually included by default if configured,
            // or don't need explicit Include like navigation properties.

        Query.Select(p => new PayslipDto
        {
            Id = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeName = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : null,
            PayPeriodStartDate = p.PayPeriodStartDate,
            PayPeriodEndDate = p.PayPeriodEndDate,
            BasicSalaryPaid = p.BasicSalaryPaid,
            TotalEarnings = p.TotalEarnings,
            TotalDeductions = p.TotalDeductions,
            NetSalary = p.NetSalary,
            GeneratedDate = p.GeneratedDate,
            Status = p.Status,
            Components = p.Components.Select(c => new PayslipComponentDto
            {
                Name = c.Name,
                Amount = c.Amount,
                Type = c.Type
            }).ToList(),
            CreatedOn = p.CreatedOn,
            LastModifiedOn = p.LastModifiedOn
        });
    }
}
