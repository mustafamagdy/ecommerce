using Ardalis.Specification;
using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec
using FSH.WebApi.Domain.HR; // For Payslip, Employee, PayslipComponent
using System.Linq;

namespace FSH.WebApi.Application.HR.Payroll.Specifications;

public class PayslipsByEmployeeSpec : EntitiesByPaginationFilterSpec<Payslip, PayslipDto>
{
    public PayslipsByEmployeeSpec(GetPayslipsByEmployeeRequest request)
        : base(request) // Handles pagination and default ordering if any from PaginationFilter
    {
        Query
            .Where(p => p.EmployeeId == request.EmployeeId)
            .Include(p => p.Employee); // Include Employee for EmployeeName
            // Payslip.Components are owned entities, usually included by default

        // Default order if not specified in request
        if (string.IsNullOrEmpty(request.OrderBy))
        {
            Query.OrderByDescending(p => p.PayPeriodEndDate);
        }

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
