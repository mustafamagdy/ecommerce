using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class ApAgingReportDto
{
    public DateTime AsOfDate { get; set; }
    public Guid? FilterSupplierId { get; set; } // To show what filter was applied
    public string? FilterSupplierName { get; set; } // For display on report header
    public List<ApAgingReportLineDto> Lines { get; set; } = new();
    public ApAgingReportTotalsDto Totals { get; set; } = new();
    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class ApAgingReportLineDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public Guid VendorInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal AmountPaid { get; set; } // Total amount paid towards this specific invoice
    public decimal AmountDue { get; set; }   // TotalInvoiceAmount - AmountPaid

    public int DaysDue { get; set; } // Positive if due in future, Negative if overdue, 0 if due today
    public string AgingBucket { get; set; } = default!; // e.g., "Current", "1-30 Days Overdue", etc.

    // Individual bucket amounts for this line. Only one of these will be populated.
    public decimal CurrentAmount { get; set; }          // Not yet due (or due today/within grace if defined)
    public decimal Overdue1To30DaysAmount { get; set; }
    public decimal Overdue31To60DaysAmount { get; set; }
    public decimal Overdue61To90DaysAmount { get; set; }
    public decimal Overdue91PlusDaysAmount { get; set; }

    // Constructor or helper method in handler will determine which bucket the AmountDue falls into.
    public ApAgingReportLineDto() { }
}

public class ApAgingReportTotalsDto
{
    public decimal GrandTotalAmountDue { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal TotalOverdue1To30DaysAmount { get; set; }
    public decimal TotalOverdue31To60DaysAmount { get; set; }
    public decimal TotalOverdue61To90DaysAmount { get; set; }
    public decimal TotalOverdue91PlusDaysAmount { get; set; }

    public int TotalInvoices { get; set; }
    public int TotalSuppliers { get; set; } // Count of unique suppliers in the report lines

    public ApAgingReportTotalsDto()
    {
        // Initialize all to zero
        GrandTotalAmountDue = 0m;
        TotalCurrentAmount = 0m;
        TotalOverdue1To30DaysAmount = 0m;
        TotalOverdue31To60DaysAmount = 0m;
        TotalOverdue61To90DaysAmount = 0m;
        TotalOverdue91PlusDaysAmount = 0m;
        TotalInvoices = 0;
        TotalSuppliers = 0;
    }
}
