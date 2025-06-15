using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class CreditMemoRegisterDto
{
    public DateTime? StartDate { get; set; } // Echo filter
    public DateTime? EndDate { get; set; }   // Echo filter
    public Guid? CustomerId { get; set; }    // Echo filter
    public string? CustomerName { get; set; } // Populated if CustomerId filter is used
    public string StatusFilter { get; set; } = default!; // String representation of the enum filter applied

    public List<CreditMemoRegisterLineDto> CreditMemos { get; set; } = new();

    public int TotalCount => CreditMemos.Count;
    public decimal GrandTotalCreditMemoAmount { get; set; } // Sum of TotalAmount for listed credit memos
    public decimal GrandTotalAppliedAmount { get; set; }    // Sum of AppliedAmount for listed credit memos
    public decimal GrandTotalAvailableBalance { get; set; } // Sum of AvailableBalance for listed credit memos

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class CreditMemoRegisterLineDto
{
    public Guid CreditMemoId { get; set; }
    public string CreditMemoNumber { get; set; } = default!;
    public DateTime Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!; // From CreditMemo.Customer.Name
    public decimal TotalAmount { get; set; }        // Original total amount of the credit memo
    public decimal AppliedAmount { get; set; }      // Calculated from applications (current state)
    public decimal AvailableBalance { get; set; }   // Calculated: TotalAmount - AppliedAmount (current state)
    public string Status { get; set; } = default!;    // From CreditMemo.Status enum, converted to string
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public Guid? OriginalCustomerInvoiceId { get; set; }
    public string? OriginalInvoiceNumber { get; set; } // Populated if OriginalCustomerInvoiceId is not null

    // For a register, usually we don't list all applications on each line.
    // If needed, a separate "Credit Memo Detail" report or drill-down would show that.
    // public List<CreditMemoApplicationDto> Applications { get; set; } = new();

    public CreditMemoRegisterLineDto() { }
}
