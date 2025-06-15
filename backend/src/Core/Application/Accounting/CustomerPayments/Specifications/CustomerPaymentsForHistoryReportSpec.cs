using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting; // For CustomerPayment
using FSH.WebApi.Domain.Operation.Customers; // For Customer
using System;
using System.Linq;

namespace FSH.WebApi.Application.Accounting.CustomerPayments.Specifications;

public class CustomerPaymentsForHistoryReportSpec : Specification<CustomerPayment>
{
    public CustomerPaymentsForHistoryReportSpec(DateTime? startDate, DateTime? endDate, Guid? customerId, Guid? paymentMethodId)
    {
        Query
            .Include(cp => cp.Customer) // For CustomerName
            .Include(cp => cp.PaymentMethod) // For PaymentMethodName
            .Include(cp => cp.AppliedInvoices); // Collection of CustomerPaymentApplication

        if (startDate.HasValue)
        {
            Query.Where(cp => cp.PaymentDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            Query.Where(cp => cp.PaymentDate <= endDate.Value.AddDays(1).AddTicks(-1)); // Inclusive of end date
        }
        if (customerId.HasValue)
        {
            Query.Where(cp => cp.CustomerId == customerId.Value);
        }
        if (paymentMethodId.HasValue)
        {
            Query.Where(cp => cp.PaymentMethodId == paymentMethodId.Value);
        }

        Query.OrderByDescending(cp => cp.PaymentDate).ThenBy(cp => cp.Customer.Name); // Default sort
    }
}
