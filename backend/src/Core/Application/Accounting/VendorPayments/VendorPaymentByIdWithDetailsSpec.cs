using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class VendorPaymentByIdWithDetailsSpec : Specification<VendorPayment, VendorPaymentDto>, ISingleResultSpecification
{
    public VendorPaymentByIdWithDetailsSpec(Guid vendorPaymentId)
    {
        Query
            .Where(vp => vp.Id == vendorPaymentId)
            .Include(vp => vp.Supplier) // To get SupplierName
            .Include(vp => vp.PaymentMethod) // To get PaymentMethodName
            .Include(vp => vp.AppliedInvoices) // To get applications
                .ThenInclude(app => app.VendorInvoice); // For each application, include its VendorInvoice to get InvoiceNumber
    }
}
