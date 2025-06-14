using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorInvoice
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorInvoices.Specifications;

public class VendorInvoicesForAgingSpec : Specification<VendorInvoice>
{
    public VendorInvoicesForAgingSpec(DateTime asOfDate, Guid? supplierId)
    {
        Query
            .Where(vi => vi.InvoiceDate <= asOfDate && // Invoice must exist as of the report date
                         vi.Status != VendorInvoiceStatus.Cancelled && // Exclude cancelled invoices
                         vi.Status != VendorInvoiceStatus.Draft) // Exclude drafts, typically only Submitted/Approved/Paid are relevant
            .Include(vi => vi.Supplier); // Include Supplier for SupplierName

        if (supplierId.HasValue)
        {
            Query.Where(vi => vi.SupplierId == supplierId.Value);
        }

        // The condition "not fully paid" is more complex with point-in-time payments.
        // It's better to fetch invoices that *could* have a balance and then calculate
        // the as-of-date paid amount in the handler.
        // So, we don't filter by "not fully paid" here in the DB query directly,
        // as that would require joining and summing payments up to AsOfDate in the database,
        // which can be complex for a specification.
        // The handler will perform this check after fetching invoices.
    }
}
