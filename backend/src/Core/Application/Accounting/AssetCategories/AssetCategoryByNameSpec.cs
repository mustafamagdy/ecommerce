using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class AssetCategoryByNameSpec : Specification<AssetCategory>, ISingleResultSpecification
{
    public AssetCategoryByNameSpec(string name) =>
        Query.Where(ac => ac.Name == name);
}
