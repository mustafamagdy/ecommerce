using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class GetFixedAssetRequest : IRequest<FixedAssetDto>
{
    public Guid Id { get; set; }

    public GetFixedAssetRequest(Guid id)
    {
        Id = id;
    }
}
