using FSH.WebApi.Application.Common.Models; // For PaginationFilter and PaginationResponse
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class GetAssetDepreciationHistoryRequest : PaginationFilter, IRequest<PaginationResponse<AssetDepreciationEntryDto>>
{
    [Required]
    public Guid FixedAssetId { get; set; }

    public DateTime? DateFrom { get; set; } // Optional filter for depreciation date range
    public DateTime? DateTo { get; set; }   // Optional filter for depreciation date range
}
