using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class GetCustomerInvoiceRequest : IRequest<CustomerInvoiceDto>
{
    public Guid Id { get; set; }

    public GetCustomerInvoiceRequest(Guid id)
    {
        Id = id;
    }
}
