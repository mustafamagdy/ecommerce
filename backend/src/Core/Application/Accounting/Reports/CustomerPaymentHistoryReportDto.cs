using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class CustomerPaymentHistoryReportDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; } // Populated if CustomerId filter is used
    public Guid? PaymentMethodId { get; set; }
    public string? PaymentMethodName { get; set; } // Populated if PaymentMethodId filter is used

    public List<CustomerPaymentHistoryLineDto> Payments { get; set; } = new();

    public int TotalPaymentsCount => Payments.Count;
    public decimal TotalPaymentsAmount { get; set; } // Sum of AmountReceived for listed payments
    public decimal TotalAppliedAmount { get; set; } // Sum of (AmountReceived - UnappliedAmount) for listed payments
    public decimal TotalUnappliedAmount { get; set; } // Sum of UnappliedAmount for listed payments


    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class CustomerPaymentHistoryLineDto
{
    public Guid CustomerPaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!; // From CustomerPayment.Customer.Name
    public decimal AmountReceived { get; set; } // Total amount of this payment
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = default!; // From CustomerPayment.PaymentMethod.Name
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public decimal AppliedAmount { get; set; } // Calculated: AmountReceived - UnappliedAmount
    public decimal UnappliedAmount { get; set; } // Calculated: from CustomerPayment.GetUnappliedAmount()
    public List<CustomerPaymentAppliedInvoiceDto> AppliedInvoices { get; set; } = new();
}

public class CustomerPaymentAppliedInvoiceDto
{
    public Guid CustomerInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!; // From related CustomerInvoice
    public DateTime InvoiceDate { get; set; }       // From related CustomerInvoice
    public decimal InvoiceTotalAmount { get; set; }
    public decimal AmountAppliedToInvoice { get; set; } // Amount of this payment applied to this specific invoice
}
