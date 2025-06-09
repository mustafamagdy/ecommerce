using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Employees.Specifications;

public class EmployeesByManagerIdSpec : Specification<Employee>
{
    public EmployeesByManagerIdSpec(Guid managerId)
    {
        Query.Where(e => e.ManagerId == managerId);
    }
}
