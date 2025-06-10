using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class SearchCustomerPaymentsRequest : PaginationFilter, IRequest<PaginationResponse<CustomerPaymentDto>>
{
    public Guid? CustomerId { get; set; }
    public DateTime? PaymentDateFrom { get; set; }
    public DateTime? PaymentDateTo { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public string? ReferenceNumberKeyword { get; set; }
    public decimal? MinimumAmountReceived { get; set; }
    public decimal? MaximumAmountReceived { get; set; }
    public bool? HasUnappliedAmount { get; set; } // Filter for payments with unapplied amounts
}
