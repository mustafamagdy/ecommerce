using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using LinqKit; // Optional: For complex predicate building if needed

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class VendorPaymentsBySearchFilterSpec : EntitiesByPaginationFilterSpec<VendorPayment, VendorPaymentDto>
{
    public VendorPaymentsBySearchFilterSpec(SearchVendorPaymentsRequest request)
        : base(request)
    {
        Query.OrderByDescending(vp => vp.PaymentDate, !request.HasOrderBy()); // Default order

        if (request.SupplierId.HasValue)
        {
            Query.Where(vp => vp.SupplierId == request.SupplierId.Value);
        }

        if (request.PaymentDateFrom.HasValue)
        {
            Query.Where(vp => vp.PaymentDate >= request.PaymentDateFrom.Value);
        }

        if (request.PaymentDateTo.HasValue)
        {
            Query.Where(vp => vp.PaymentDate <= request.PaymentDateTo.Value.AddDays(1).AddTicks(-1)); // Include whole day
        }

        if (request.PaymentMethodId.HasValue)
        {
            Query.Where(vp => vp.PaymentMethodId == request.PaymentMethodId.Value);
        }

        if (!string.IsNullOrEmpty(request.ReferenceNumberKeyword))
        {
            Query.Search(vp => vp.ReferenceNumber, "%" + request.ReferenceNumberKeyword + "%");
        }

        // Ensure related data needed for DTO is included
        Query
            .Include(vp => vp.Supplier)
            .Include(vp => vp.PaymentMethod)
            .Include(vp => vp.AppliedInvoices)
                .ThenInclude(app => app.VendorInvoice);
    }
}
