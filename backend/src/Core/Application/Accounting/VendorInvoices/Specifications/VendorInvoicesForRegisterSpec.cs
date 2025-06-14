using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorInvoice
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorInvoices.Specifications;

public class VendorInvoicesForRegisterSpec : Specification<VendorInvoice>
{
    public VendorInvoicesForRegisterSpec(DateTime? startDate, DateTime? endDate, Guid? supplierId)
    {
        Query.Include(vi => vi.Supplier); // Include Supplier for SupplierName and ContactInfo

        if (startDate.HasValue)
        {
            Query.Where(vi => vi.InvoiceDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            Query.Where(vi => vi.InvoiceDate <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }

        if (supplierId.HasValue)
        {
            Query.Where(vi => vi.SupplierId == supplierId.Value);
        }

        // Exclude Draft/Cancelled invoices as they are generally not part of a register of posted payables.
        Query.Where(vi => vi.Status != VendorInvoiceStatus.Draft && vi.Status != VendorInvoiceStatus.Cancelled);

        Query.OrderBy(vi => vi.Supplier.Name).ThenBy(vi => vi.InvoiceDate); // Default sort
    }
}
