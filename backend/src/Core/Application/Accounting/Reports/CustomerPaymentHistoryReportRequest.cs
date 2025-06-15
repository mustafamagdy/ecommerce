using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class CustomerPaymentHistoryReportRequest : IRequest<CustomerPaymentHistoryReportDto>
{
    public DateTime? StartDate { get; set; } // Filter by PaymentDate
    public DateTime? EndDate { get; set; }   // Filter by PaymentDate
    public Guid? CustomerId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    // public string? ReferenceNumberKeyword { get; set; } // Optional future filter
    // public bool IncludeUnappliedPaymentsOnly { get; set; } = false; // Optional future filter
}
