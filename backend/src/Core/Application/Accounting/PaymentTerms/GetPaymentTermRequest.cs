using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class GetPaymentTermRequest : IRequest<PaymentTermDto>
{
    public Guid Id { get; set; }

    public GetPaymentTermRequest(Guid id)
    {
        Id = id;
    }
}
