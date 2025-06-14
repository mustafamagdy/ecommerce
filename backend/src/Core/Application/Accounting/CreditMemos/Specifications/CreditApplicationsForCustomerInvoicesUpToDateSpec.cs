using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CreditMemoApplication, CreditMemo, CreditMemoStatus
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CreditMemos.Specifications;

public class CreditApplicationsForCustomerInvoicesUpToDateSpec : Specification<CreditMemoApplication>
{
    // Fetches all credit memo applications for a list of customerInvoiceIds
    // where the credit memo date is on or before asOfDate and credit memo is in an applicable status.
    public CreditApplicationsForCustomerInvoicesUpToDateSpec(IEnumerable<Guid> customerInvoiceIds, DateTime asOfDate)
    {
        Query
            .Where(app => customerInvoiceIds.Contains(app.CustomerInvoiceId) &&
                          app.CreditMemo.Date <= asOfDate &&
                          (app.CreditMemo.Status == CreditMemoStatus.Approved ||
                           app.CreditMemo.Status == CreditMemoStatus.PartiallyApplied ||
                           app.CreditMemo.Status == CreditMemoStatus.Applied)) // Only consider effective credit memos
            .Include(app => app.CreditMemo); // Include CreditMemo to access Date and Status
    }

    // Overload for a single invoice ID, if needed
    public CreditApplicationsForCustomerInvoicesUpToDateSpec(Guid customerInvoiceId, DateTime asOfDate)
    {
        Query
            .Where(app => app.CustomerInvoiceId == customerInvoiceId &&
                          app.CreditMemo.Date <= asOfDate &&
                          (app.CreditMemo.Status == CreditMemoStatus.Approved ||
                           app.CreditMemo.Status == CreditMemoStatus.PartiallyApplied ||
                           app.CreditMemo.Status == CreditMemoStatus.Applied))
            .Include(app => app.CreditMemo);
    }
}
