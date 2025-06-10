using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using System.Linq; // Required for .Sum()
using LinqKit; // Optional

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class CreditMemosBySearchFilterSpec : EntitiesByPaginationFilterSpec<CreditMemo, CreditMemoDto>
{
    public CreditMemosBySearchFilterSpec(SearchCreditMemosRequest request)
        : base(request)
    {
        Query.OrderByDescending(cm => cm.Date, !request.HasOrderBy());

        if (request.CustomerId.HasValue)
        {
            Query.Where(cm => cm.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrEmpty(request.CreditMemoNumberKeyword))
        {
            Query.Search(cm => cm.CreditMemoNumber, "%" + request.CreditMemoNumberKeyword + "%");
        }

        if (request.DateFrom.HasValue)
        {
            Query.Where(cm => cm.Date >= request.DateFrom.Value);
        }
        if (request.DateTo.HasValue)
        {
            Query.Where(cm => cm.Date <= request.DateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<CreditMemoStatus>(request.Status, true, out var statusEnum))
            {
                Query.Where(cm => cm.Status == statusEnum);
            }
        }

        if (request.MinimumTotalAmount.HasValue)
        {
            Query.Where(cm => cm.TotalAmount >= request.MinimumTotalAmount.Value);
        }
        if (request.MaximumTotalAmount.HasValue)
        {
            Query.Where(cm => cm.TotalAmount <= request.MaximumTotalAmount.Value);
        }

        if (request.OriginalCustomerInvoiceId.HasValue)
        {
            Query.Where(cm => cm.OriginalCustomerInvoiceId == request.OriginalCustomerInvoiceId.Value);
        }

        if (request.HasAvailableBalance.HasValue)
        {
            if (request.HasAvailableBalance.Value)
            {
                // TotalAmount > Sum of AppliedInvoices.AmountApplied
                Query.Where(cm => cm.TotalAmount > cm.Applications.Sum(a => a.AmountApplied));
            }
            else
            {
                // TotalAmount <= Sum of AppliedInvoices.AmountApplied (fully applied or over-applied, though latter shouldn't happen)
                Query.Where(cm => cm.TotalAmount <= cm.Applications.Sum(a => a.AmountApplied));
            }
        }

        // Include related data needed for DTO list view
        Query
            // .Include(cm => cm.Customer) // For CustomerName - fetched in handler
            .Include(cm => cm.Applications)
                .ThenInclude(app => app.CustomerInvoice); // For InvoiceNumber in applications list preview
            // OriginalCustomerInvoiceNumber also fetched in handler.
    }
}
