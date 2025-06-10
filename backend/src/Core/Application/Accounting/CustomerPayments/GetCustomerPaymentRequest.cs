using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class GetCustomerPaymentRequest : IRequest<CustomerPaymentDto>
{
    public Guid Id { get; set; }

    public GetCustomerPaymentRequest(Guid id)
    {
        Id = id;
    }
}
