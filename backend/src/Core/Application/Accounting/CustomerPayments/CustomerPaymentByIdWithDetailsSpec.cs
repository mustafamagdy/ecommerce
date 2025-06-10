using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // Assuming Customer is needed by name
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CustomerPaymentByIdWithDetailsSpec : Specification<CustomerPayment, CustomerPaymentDto>, ISingleResultSpecification
{
    public CustomerPaymentByIdWithDetailsSpec(Guid customerPaymentId)
    {
        Query
            .Where(cp => cp.Id == customerPaymentId)
            // .Include(cp => cp.Customer) // If Customer navigation property exists and is needed directly
            .Include(cp => cp.PaymentMethod) // To get PaymentMethodName
            .Include(cp => cp.AppliedInvoices) // To get applications
                .ThenInclude(app => app.CustomerInvoice); // For each application, include its CustomerInvoice
    }
}
