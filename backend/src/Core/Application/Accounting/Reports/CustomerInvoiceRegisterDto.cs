using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class CustomerInvoiceRegisterDto
{
    public DateTime? StartDate { get; set; } // Report parameter echo
    public DateTime? EndDate { get; set; }   // Report parameter echo
    public Guid? CustomerId { get; set; }    // Report parameter echo
    public string? CustomerName { get; set; } // For display if single customer filter applied
    public string StatusFilter { get; set; } = default!; // String representation of the enum filter applied
    public DateTime AsOfDate { get; set; }      // Report parameter echo

    public List<CustomerInvoiceRegisterLineDto> Invoices { get; set; } = new();

    public int TotalCount => Invoices.Count;
    public decimal GrandTotalAmount { get; set; }           // Sum of TotalAmount for listed invoices
    public decimal GrandTotalPaidOrCreditedAmount { get; set; } // Sum of AmountPaidOrCredited for listed invoices
    public decimal GrandTotalAmountDue { get; set; }        // Sum of AmountDue for listed invoices

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class CustomerInvoiceRegisterLineDto
{
    public Guid CustomerInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string? CustomerContactInfo { get; set; } // Optional: e.g., Customer.Email or Customer.PhoneNumber
    public decimal TotalAmount { get; set; }           // Original total amount of the invoice
    public decimal AmountPaidOrCredited { get; set; }  // Total payments + credits applied as of AsOfDate
    public decimal AmountDue { get; set; }             // Calculated as of AsOfDate (TotalAmount - AmountPaidOrCredited)
    public string Status { get; set; } = default!;      // e.g., "Open", "Paid", "Partially Paid", "Overdue"
    public DateTime? LastPaymentOrCreditDate { get; set; } // Date of the most recent payment or credit applied (up to AsOfDate)
    public int DaysOverdue { get; set; }               // Calculated: (AsOfDate - DueDate).Days. Positive if overdue, zero or negative otherwise.

    public CustomerInvoiceRegisterLineDto() { }
}
