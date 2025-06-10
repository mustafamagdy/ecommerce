using FSH.WebApi.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CustomerPaymentDto : IDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; } // For display
    public DateTime PaymentDate { get; set; }
    public decimal AmountReceived { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string? PaymentMethodName { get; set; } // For display
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public decimal UnappliedAmount { get; set; } // Calculated: AmountReceived - Sum(AppliedInvoices.AmountApplied)
    public List<CustomerPaymentApplicationDto> AppliedInvoices { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public class CustomerPaymentApplicationDto : IDto
{
    public Guid Id { get; set; }
    // CustomerPaymentId might not be needed if always a child of CustomerPaymentDto
    // public Guid CustomerPaymentId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public string? CustomerInvoiceNumber { get; set; } // For display
    public decimal AmountApplied { get; set; }
}
