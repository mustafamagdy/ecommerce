using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class DeleteVendorInvoiceRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteVendorInvoiceRequest(Guid id)
    {
        Id = id;
    }
}
