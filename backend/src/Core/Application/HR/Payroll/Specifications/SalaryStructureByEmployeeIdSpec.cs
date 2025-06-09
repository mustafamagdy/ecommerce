using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Payroll.Specifications;

public class SalaryStructureByEmployeeIdSpec : Specification<SalaryStructure>, ISingleResultSpecification
{
    public SalaryStructureByEmployeeIdSpec(Guid employeeId) =>
        Query.Where(ss => ss.EmployeeId == employeeId)
             .Include(ss => ss.Employee); // Include Employee for EmployeeName
}
