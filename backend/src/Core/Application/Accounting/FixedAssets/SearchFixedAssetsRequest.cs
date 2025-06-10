using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class SearchFixedAssetsRequest : PaginationFilter, IRequest<PaginationResponse<FixedAssetDto>>
{
    public string? AssetNumberKeyword { get; set; } // Search in AssetNumber
    public string? AssetNameKeyword { get; set; }   // Search in AssetName or Description
    public Guid? AssetCategoryId { get; set; }
    public string? Status { get; set; } // Parsed to FixedAssetStatus enum
    public DateTime? PurchaseDateFrom { get; set; }
    public DateTime? PurchaseDateTo { get; set; }
    public Guid? DepreciationMethodId { get; set; }
    public string? LocationKeyword { get; set; }
}
