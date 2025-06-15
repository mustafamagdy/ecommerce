using MediatR;
using System;
using FSH.WebApi.Application.Common.Validation; // For [ValidateNotEmpty]

namespace FSH.WebApi.Application.Accounting.Reports;

public enum OutstandingTransactionTypeFilter
{
    All,
    UnclearedChecksOrDebits,  // System-side debits (payments, withdrawals) not cleared on bank statement
    DepositsInTransitOrCredits // System-side credits (deposits, receipts) not cleared on bank statement
}

public class OutstandingTransactionsReportRequest : IRequest<OutstandingTransactionsReportDto>
{
    [ValidateNotEmpty]
    public Guid BankAccountId { get; set; }

    [ValidateNotEmpty]
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow; // To determine "outstanding as of this date"

    public OutstandingTransactionTypeFilter TypeFilter { get; set; } = OutstandingTransactionTypeFilter.All;

    // How far back from AsOfDate to look for GL entries. Defaults to a reasonable period.
    public int LookbackWindowDays { get; set; } = 90;
}
