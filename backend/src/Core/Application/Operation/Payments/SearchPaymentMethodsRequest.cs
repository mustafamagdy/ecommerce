using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Payments;

public class SearchPaymentMethodsRequest : PaginationFilter, IRequest<PaginationResponse<PaymentMethodDto>>
{

}

public class SearchPaymentMethodsRequestSpec : EntitiesByPaginationFilterSpec<PaymentMethod, PaymentMethodDto>
{
    public SearchPaymentMethodsRequestSpec(SearchPaymentMethodsRequest request)
        : base(request) =>
        Query
            .OrderBy(c => c.Name, !request.HasOrderBy());
}
public class GetPaymentMethodsHandler : IRequestHandler<SearchPaymentMethodsRequest, PaginationResponse<PaymentMethodDto>>
{
    private readonly IReadRepository<PaymentMethod> _repository;
    private readonly IStringLocalizer<GetPaymentMethodsHandler> _t;

    public GetPaymentMethodsHandler(IReadRepository<PaymentMethod> repository, IStringLocalizer<GetPaymentMethodsHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<PaginationResponse<PaymentMethodDto>> Handle(SearchPaymentMethodsRequest request, CancellationToken cancellationToken)
    {
        var spec= new SearchPaymentMethodsRequestSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}

public class PaymentMethodDto:IDto
{
    public Guid Id { get; }
    public string Name { get; }

    public PaymentMethodDto(Guid Id, string name)
    {
        this.Id = Id;
        Name = name;
    }
}