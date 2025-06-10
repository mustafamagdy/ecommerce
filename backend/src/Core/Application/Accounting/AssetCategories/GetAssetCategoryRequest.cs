using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class GetAssetCategoryRequest : IRequest<AssetCategoryDto>
{
    public Guid Id { get; set; }

    public GetAssetCategoryRequest(Guid id)
    {
        Id = id;
    }
}
