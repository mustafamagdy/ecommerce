using MediatR;
using System;
using System.Collections.Generic; // Not strictly needed now, but for future filter consistency

namespace FSH.WebApi.Application.Accounting.Reports;

public enum VendorInvoiceRegisterStatusFilter
{
    All,
    Open,       // Includes Partially Paid and not yet due but unpaid
    Paid,       // Fully Paid
    Overdue,    // DueDate < AsOfDate and not Fully Paid
    Unpaid      // Not Fully Paid (combines Open and Overdue)
}

public class VendorInvoiceRegisterRequest : IRequest<VendorInvoiceRegisterDto>
{
    public DateTime? StartDate { get; set; } // Filter by InvoiceDate range start
    public DateTime? EndDate { get; set; }   // Filter by InvoiceDate range end

    public Guid? SupplierId { get; set; }

    public VendorInvoiceRegisterStatusFilter StatusFilter { get; set; } = VendorInvoiceRegisterStatusFilter.All;

    // AsOfDate is crucial for determining status like "Open" or "Overdue" accurately.
    // It defines the point in time for which the payment status of invoices is evaluated.
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
}
