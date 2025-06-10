using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class DeletePaymentTermRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeletePaymentTermRequest(Guid id)
    {
        Id = id;
    }
}
