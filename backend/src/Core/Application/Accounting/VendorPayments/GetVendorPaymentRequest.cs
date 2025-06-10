using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class GetVendorPaymentRequest : IRequest<VendorPaymentDto> // Assuming it returns VendorPaymentDto
{
    public Guid Id { get; set; }

    public GetVendorPaymentRequest(Guid id)
    {
        Id = id;
    }
}
