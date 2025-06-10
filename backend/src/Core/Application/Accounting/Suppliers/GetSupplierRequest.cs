using MediatR;
using System;
using FSH.WebApi.Application.Accounting.Suppliers; // For SupplierDto

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class GetSupplierRequest : IRequest<SupplierDto>
{
    public Guid Id { get; set; }

    public GetSupplierRequest(Guid id)
    {
        Id = id;
    }
}
