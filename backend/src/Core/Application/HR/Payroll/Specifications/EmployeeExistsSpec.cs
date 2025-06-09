using Ardalis.Specification;
using FSH.WebApi.Domain.HR;

namespace FSH.WebApi.Application.HR.Payroll.Specifications;

public class EmployeeExistsSpec : Specification<Employee>, ISingleResultSpecification
{
    public EmployeeExistsSpec(Guid id) => Query.Where(e => e.Id == id);
}
