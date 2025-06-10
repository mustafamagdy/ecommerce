using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class SearchAssetCategoriesRequest : PaginationFilter, IRequest<PaginationResponse<AssetCategoryDto>>
{
    public string? NameKeyword { get; set; }
    public bool? IsActive { get; set; }
    public Guid? DefaultDepreciationMethodId { get; set; }
}
