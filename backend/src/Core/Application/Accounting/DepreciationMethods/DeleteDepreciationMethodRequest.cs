using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class DeleteDepreciationMethodRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteDepreciationMethodRequest(Guid id)
    {
        Id = id;
    }
}
