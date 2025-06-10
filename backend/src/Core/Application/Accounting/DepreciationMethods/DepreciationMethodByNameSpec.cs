using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class DepreciationMethodByNameSpec : Specification<DepreciationMethod>, ISingleResultSpecification
{
    public DepreciationMethodByNameSpec(string name) =>
        Query.Where(dm => dm.Name == name);
}
