using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class DeleteCustomerInvoiceRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteCustomerInvoiceRequest(Guid id)
    {
        Id = id;
    }
}
