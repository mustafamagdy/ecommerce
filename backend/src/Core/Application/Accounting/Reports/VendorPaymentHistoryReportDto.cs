using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class VendorPaymentHistoryReportDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; } // Populated if SupplierId filter is used
    public Guid? PaymentMethodId { get; set; }
    public string? PaymentMethodName { get; set; } // Populated if PaymentMethodId filter is used

    public List<VendorPaymentHistoryLineDto> Payments { get; set; } = new();

    public int TotalPaymentsCount => Payments.Count;
    public decimal TotalPaymentsAmount { get; set; } // Sum of AmountPaid for listed payments

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class VendorPaymentHistoryLineDto
{
    public Guid VendorPaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = default!; // From VendorPayment.Supplier.Name
    public decimal AmountPaid { get; set; } // Total amount of this payment
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = default!; // From VendorPayment.PaymentMethod.Name
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public List<VendorPaymentAppliedInvoiceDto> AppliedInvoices { get; set; } = new(); // Details of invoices this payment was applied to
}

public class VendorPaymentAppliedInvoiceDto
{
    public Guid VendorInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!; // From related VendorInvoice
    public DateTime InvoiceDate { get; set; }       // From related VendorInvoice
    public decimal InvoiceTotalAmount { get; set; }  // Added: Original total of the invoice
    public decimal AmountAppliedToInvoice { get; set; } // Amount of this payment applied to this specific invoice
}
