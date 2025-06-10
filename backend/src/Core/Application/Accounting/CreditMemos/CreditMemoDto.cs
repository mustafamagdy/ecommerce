using FSH.WebApi.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class CreditMemoDto : IDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; } // For display
    public string CreditMemoNumber { get; set; } = default!;
    public DateTime Date { get; set; }
    public string Reason { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public decimal AppliedAmount { get; set; } // Calculated: Sum(Applications.AmountApplied)
    public decimal AvailableBalance { get; set; } // Calculated: TotalAmount - AppliedAmount
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!; // Mapped from CreditMemoStatus enum
    public string? Notes { get; set; }
    public Guid? OriginalCustomerInvoiceId { get; set; }
    public string? OriginalCustomerInvoiceNumber { get; set; } // For display
    public List<CreditMemoApplicationDto> Applications { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public class CreditMemoApplicationDto : IDto
{
    public Guid Id { get; set; }
    // CreditMemoId might not be needed if always a child of CreditMemoDto
    // public Guid CreditMemoId { get; set; }
    public Guid CustomerInvoiceId { get; set; }
    public string? CustomerInvoiceNumber { get; set; } // For display
    public decimal AmountApplied { get; set; }
}
