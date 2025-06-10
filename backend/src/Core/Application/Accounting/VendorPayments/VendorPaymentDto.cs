using FSH.WebApi.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class VendorPaymentDto : IDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; } // For display
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string? PaymentMethodName { get; set; } // For display
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public List<VendorPaymentApplicationDto> AppliedInvoices { get; set; } = new();
    public DateTime CreatedOn { get; set; } // From AuditableEntity
}

public class VendorPaymentApplicationDto : IDto
{
    public Guid Id { get; set; }
    public Guid VendorPaymentId { get; set; } // May not be needed if always a child of VendorPaymentDto
    public Guid VendorInvoiceId { get; set; }
    public string? VendorInvoiceNumber { get; set; } // For display
    public decimal AmountApplied { get; set; }
}
