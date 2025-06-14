using System;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.Reports;

public class ArAgingReportDto
{
    public DateTime AsOfDate { get; set; }
    public Guid? CustomerId { get; set; } // Echo filter
    public string? CustomerName { get; set; } // Populated if CustomerId filter is used

    public List<ArAgingReportLineDto> Lines { get; set; } = new();
    public ArAgingReportTotalsDto Totals { get; set; } = new();

    public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
}

public class ArAgingReportLineDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!; // From CustomerInvoice.Customer.Name (or direct lookup)
    public Guid CustomerInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal AmountPaid { get; set; } // Total payments + credits applied to this specific invoice as of AsOfDate
    public decimal AmountDue { get; set; }   // TotalInvoiceAmount - AmountPaid

    public int DaysDueOrOverdue { get; set; } // Positive if due in future (DaysDue), Negative if overdue (DaysOverdue as positive)
                                          // Example: (DueDate.Date - AsOfDate.Date).Days
                                          // Handler will calculate actual "DaysOverdue" based on this.
    public string AgingBucket { get; set; } = default!; // e.g., "Current", "1-30 Days Overdue", etc.

    // Individual bucket amounts for this line. Only one of these will be populated with AmountDue.
    public decimal CurrentAmount { get; set; }          // Not yet due (or due today/within grace if defined)
    public decimal Overdue1To30DaysAmount { get; set; }
    public decimal Overdue31To60DaysAmount { get; set; }
    public decimal Overdue61To90DaysAmount { get; set; }
    public decimal Overdue91PlusDaysAmount { get; set; }

    public ArAgingReportLineDto() { }
}

public class ArAgingReportTotalsDto
{
    public decimal GrandTotalAmountDue { get; set; } // Sum of all AmountDue from lines
    public decimal TotalCurrentAmount { get; set; }
    public decimal TotalOverdue1To30DaysAmount { get; set; }
    public decimal TotalOverdue31To60DaysAmount { get; set; }
    public decimal TotalOverdue61To90DaysAmount { get; set; }
    public decimal TotalOverdue91PlusDaysAmount { get; set; }

    public int TotalInvoices { get; set; }
    public int TotalCustomers { get; set; } // Count of unique customers in the report lines

    public ArAgingReportTotalsDto()
    {
        GrandTotalAmountDue = 0m;
        TotalCurrentAmount = 0m;
        TotalOverdue1To30DaysAmount = 0m;
        TotalOverdue31To60DaysAmount = 0m;
        TotalOverdue61To90DaysAmount = 0m;
        TotalOverdue91PlusDaysAmount = 0m;
        TotalInvoices = 0;
        TotalCustomers = 0;
    }
}
