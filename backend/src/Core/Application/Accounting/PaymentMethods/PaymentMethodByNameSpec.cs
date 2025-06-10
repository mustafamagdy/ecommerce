using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class PaymentMethodByNameSpec : Specification<PaymentMethod>, ISingleResultSpecification
{
    public PaymentMethodByNameSpec(string name) =>
        Query.Where(pm => pm.Name == name);
}
