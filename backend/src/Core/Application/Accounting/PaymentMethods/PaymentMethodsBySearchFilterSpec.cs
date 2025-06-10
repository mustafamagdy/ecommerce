using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Domain.Accounting;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class PaymentMethodsBySearchFilterSpec : EntitiesByPaginationFilterSpec<PaymentMethod, PaymentMethodDto>
{
    public PaymentMethodsBySearchFilterSpec(SearchPaymentMethodsRequest request)
        : base(request)
    {
        Query.OrderBy(pm => pm.Name, !request.HasOrderBy());

        if (!string.IsNullOrEmpty(request.NameKeyword))
        {
            Query.Search(pm => pm.Name, "%" + request.NameKeyword + "%")
                 .Search(pm => pm.Description, "%" + request.NameKeyword + "%");
        }

        if (request.IsActive.HasValue)
        {
            Query.Where(pm => pm.IsActive == request.IsActive.Value);
        }
    }
}
