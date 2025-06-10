using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using System.Linq; // Required for .Sum()
using LinqKit; // Optional: For complex predicate building if needed

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CustomerPaymentsBySearchFilterSpec : EntitiesByPaginationFilterSpec<CustomerPayment, CustomerPaymentDto>
{
    public CustomerPaymentsBySearchFilterSpec(SearchCustomerPaymentsRequest request)
        : base(request)
    {
        Query.OrderByDescending(cp => cp.PaymentDate, !request.HasOrderBy()); // Default order

        if (request.CustomerId.HasValue)
        {
            Query.Where(cp => cp.CustomerId == request.CustomerId.Value);
        }

        if (request.PaymentDateFrom.HasValue)
        {
            Query.Where(cp => cp.PaymentDate >= request.PaymentDateFrom.Value);
        }

        if (request.PaymentDateTo.HasValue)
        {
            Query.Where(cp => cp.PaymentDate <= request.PaymentDateTo.Value.AddDays(1).AddTicks(-1));
        }

        if (request.PaymentMethodId.HasValue)
        {
            Query.Where(cp => cp.PaymentMethodId == request.PaymentMethodId.Value);
        }

        if (!string.IsNullOrEmpty(request.ReferenceNumberKeyword))
        {
            Query.Search(cp => cp.ReferenceNumber, "%" + request.ReferenceNumberKeyword + "%");
        }

        if (request.MinimumAmountReceived.HasValue)
        {
            Query.Where(cp => cp.AmountReceived >= request.MinimumAmountReceived.Value);
        }

        if (request.MaximumAmountReceived.HasValue)
        {
            Query.Where(cp => cp.AmountReceived <= request.MaximumAmountReceived.Value);
        }

        if (request.HasUnappliedAmount.HasValue)
        {
            if (request.HasUnappliedAmount.Value)
            {
                // Sum of AppliedInvoices.AmountApplied < AmountReceived
                Query.Where(cp => cp.AppliedInvoices.Sum(a => a.AmountApplied) < cp.AmountReceived);
            }
            else
            {
                // Sum of AppliedInvoices.AmountApplied == AmountReceived
                Query.Where(cp => cp.AppliedInvoices.Sum(a => a.AmountApplied) == cp.AmountReceived);
            }
        }

        // Ensure related data needed for DTO is included for list view
        Query
            // .Include(cp => cp.Customer) // For CustomerName - will be fetched separately in handler
            .Include(cp => cp.PaymentMethod) // For PaymentMethodName
            .Include(cp => cp.AppliedInvoices)
                .ThenInclude(app => app.CustomerInvoice); // For InvoiceNumber in applications
    }
}
