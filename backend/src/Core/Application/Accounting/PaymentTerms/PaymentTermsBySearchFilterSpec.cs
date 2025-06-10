using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class PaymentTermsBySearchFilterSpec : EntitiesByPaginationFilterSpec<PaymentTerm, PaymentTermDto>
{
    public PaymentTermsBySearchFilterSpec(SearchPaymentTermsRequest request)
        : base(request)
    {
        Query.OrderBy(pt => pt.Name, !request.HasOrderBy()); // Default order

        if (!string.IsNullOrEmpty(request.NameKeyword))
        {
            Query.Search(pt => pt.Name, "%" + request.NameKeyword + "%")
                 .Search(pt => pt.Description, "%" + request.NameKeyword + "%");
        }

        if (request.IsActive.HasValue)
        {
            Query.Where(pt => pt.IsActive == request.IsActive.Value);
        }

        if (request.ExactDaysUntilDue.HasValue)
        {
            Query.Where(pt => pt.DaysUntilDue == request.ExactDaysUntilDue.Value);
        }
    }
}
