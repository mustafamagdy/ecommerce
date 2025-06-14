using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorInvoice
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorInvoices.Specifications;

public class VendorInvoicesByIdsSpec : Specification<VendorInvoice>
{
    public VendorInvoicesByIdsSpec(IEnumerable<Guid> ids)
    {
        Query
            .Where(vi => ids.Contains(vi.Id));
        // Select only necessary fields if possible, though full entity is often fetched by default with IRepository<T>.
        // For this report, we need Id, InvoiceNumber, InvoiceDate, TotalAmount.
        // If this spec were returning a DTO, projection would be defined here.
        // Since it returns VendorInvoice, the handler will access these properties.
    }
}
