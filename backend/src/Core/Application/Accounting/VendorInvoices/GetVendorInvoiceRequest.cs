using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class GetVendorInvoiceRequest : IRequest<VendorInvoiceDto>
{
    public Guid Id { get; set; }

    public GetVendorInvoiceRequest(Guid id)
    {
        Id = id;
    }
}
