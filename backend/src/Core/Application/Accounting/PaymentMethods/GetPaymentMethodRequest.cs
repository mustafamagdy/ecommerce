using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class GetPaymentMethodRequest : IRequest<PaymentMethodDto>
{
    public Guid Id { get; set; }

    public GetPaymentMethodRequest(Guid id)
    {
        Id = id;
    }
}
