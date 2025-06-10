using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class DeleteAssetCategoryRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteAssetCategoryRequest(Guid id)
    {
        Id = id;
    }
}
