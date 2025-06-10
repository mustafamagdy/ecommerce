using Ardalis.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class PaymentTermByNameSpec : Specification<PaymentTerm>, ISingleResultSpecification
{
    public PaymentTermByNameSpec(string name) =>
        Query.Where(pt => pt.Name == name);
}
