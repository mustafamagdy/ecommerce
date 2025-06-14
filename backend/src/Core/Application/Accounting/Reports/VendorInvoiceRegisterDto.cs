using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class VendorInvoiceRegisterDto
{
    public DateTime? StartDate { get; set; } // Report parameter echo
    public DateTime? EndDate { get; set; }   // Report parameter echo
    public Guid? SupplierId { get; set; }    // Report parameter echo
    public string? SupplierName { get; set; } // For display if single supplier filter applied
    public string StatusFilter { get; set; } = default!; // String representation of the enum filter applied
    public DateTime AsOfDate { get; set; }      // Report parameter echo

    public List<VendorInvoiceRegisterLineDto> Invoices { get; set; } = new();

    public int TotalCount => Invoices.Count;
    public decimal GrandTotalAmount { get; set; }    // Sum of TotalAmount for listed invoices
    public decimal GrandTotalPaidAmount { get; set; } // Sum of AmountPaid for listed invoices
    public decimal GrandTotalAmountDue { get; set; }  // Sum of AmountDue for listed invoices

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class VendorInvoiceRegisterLineDto
{
    public Guid VendorInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? SupplierContactInfo { get; set; } // Added for more supplier detail on line
    public decimal TotalAmount { get; set; }    // Original total amount of the invoice
    public decimal AmountPaid { get; set; }     // Calculated as of AsOfDate
    public decimal AmountDue { get; set; }      // Calculated as of AsOfDate (TotalAmount - AmountPaid)
    public string Status { get; set; } = default!; // e.g., "Open", "Paid", "Partially Paid", "Overdue"
    public DateTime? LastPaymentDate { get; set; } // Date of the most recent payment towards this invoice (up to AsOfDate)
    public int DaysOverdue { get; set; } // Calculated: AsOfDate - DueDate. Positive if overdue, zero or negative otherwise.

    public VendorInvoiceRegisterLineDto() { }
}
