using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public class VendorPaymentHistoryReportRequest : IRequest<VendorPaymentHistoryReportDto>
{
    public DateTime? StartDate { get; set; } // Filter by PaymentDate
    public DateTime? EndDate { get; set; }   // Filter by PaymentDate
    public Guid? SupplierId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    // public string? ReferenceNumberKeyword { get; set; } // Optional future filter
}
