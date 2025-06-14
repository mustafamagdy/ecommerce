using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For VendorPayment
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorPayments.Specifications;

public class VendorPaymentsForHistoryReportSpec : Specification<VendorPayment>
{
    public VendorPaymentsForHistoryReportSpec(DateTime? startDate, DateTime? endDate, Guid? supplierId, Guid? paymentMethodId)
    {
        Query
            .Include(vp => vp.Supplier)
            .Include(vp => vp.PaymentMethod)
            .Include(vp => vp.AppliedInvoices); // Collection of VendorPaymentApplication

        if (startDate.HasValue)
        {
            Query.Where(vp => vp.PaymentDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            Query.Where(vp => vp.PaymentDate <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }
        if (supplierId.HasValue)
        {
            Query.Where(vp => vp.SupplierId == supplierId.Value);
        }
        if (paymentMethodId.HasValue)
        {
            Query.Where(vp => vp.PaymentMethodId == paymentMethodId.Value);
        }

        Query.OrderByDescending(vp => vp.PaymentDate).ThenBy(vp => vp.Supplier.Name); // Default sort
    }
}
