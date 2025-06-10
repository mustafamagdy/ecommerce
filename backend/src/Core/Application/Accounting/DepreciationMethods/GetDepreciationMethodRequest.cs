using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class GetDepreciationMethodRequest : IRequest<DepreciationMethodDto>
{
    public Guid Id { get; set; }

    public GetDepreciationMethodRequest(Guid id)
    {
        Id = id;
    }
}
