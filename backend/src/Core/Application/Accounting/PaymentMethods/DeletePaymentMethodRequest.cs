using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class DeletePaymentMethodRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeletePaymentMethodRequest(Guid id)
    {
        Id = id;
    }
}
