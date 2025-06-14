using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public enum CustomerInvoiceRegisterStatusFilter
{
    All,
    Open,       // Not fully paid/credited (includes Partially Paid and unpaid not yet due)
    Paid,       // Fully paid/credited
    Overdue,    // DueDate < AsOfDate and not Fully Paid/Credited
    Unpaid      // No payments or credits applied yet, but could be current or overdue
}

public class CustomerInvoiceRegisterRequest : IRequest<CustomerInvoiceRegisterDto>
{
    public DateTime? StartDate { get; set; } // Filter by InvoiceDate range start
    public DateTime? EndDate { get; set; }   // Filter by InvoiceDate range end

    public Guid? CustomerId { get; set; }

    public CustomerInvoiceRegisterStatusFilter StatusFilter { get; set; } = CustomerInvoiceRegisterStatusFilter.All;

    // AsOfDate is crucial for determining status like "Open" or "Overdue" accurately.
    // It defines the point in time for which the payment/credit status of invoices is evaluated.
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
}
