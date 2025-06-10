using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class FixedAssetByAssetNumberSpec : Specification<FixedAsset>, ISingleResultSpecification
{
    public FixedAssetByAssetNumberSpec(string assetNumber) =>
        Query.Where(fa => fa.AssetNumber == assetNumber);
}
