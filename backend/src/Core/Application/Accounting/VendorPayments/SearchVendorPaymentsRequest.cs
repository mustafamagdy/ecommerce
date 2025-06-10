using FSH.WebApi.Application.Common.Models; // For PaginationFilter and PaginationResponse
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class SearchVendorPaymentsRequest : PaginationFilter, IRequest<PaginationResponse<VendorPaymentDto>>
{
    public Guid? SupplierId { get; set; }
    public DateTime? PaymentDateFrom { get; set; }
    public DateTime? PaymentDateTo { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? ReferenceNumberKeyword { get; set; } // Search within ReferenceNumber
    // public decimal? MinimumAmountPaid { get; set; } // Example of other potential filters
    // public decimal? MaximumAmountPaid { get; set; }
}
