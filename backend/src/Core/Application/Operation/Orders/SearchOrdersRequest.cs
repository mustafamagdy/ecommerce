using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class SearchOrdersRequest : PaginationFilter, IRequest<PaginationResponse<OrderDto>>
{
  public string? OrderNumber { get; set; }
}

public class OrdersBySearchRequestSpec : EntitiesByPaginationFilterSpec<Order, OrderDto>
{
  public OrdersBySearchRequestSpec(SearchOrdersRequest request)
    : base(request) =>
    Query
      .Where(a => request.OrderNumber != null && a.OrderNumber == request.OrderNumber)
      .OrderBy(c => c.OrderNumber, !request.HasOrderBy());
}

public class SearchOrdersRequestHandler : IRequestHandler<SearchOrdersRequest, PaginationResponse<OrderDto>>
{
  private readonly IReadRepository<Order> _repository;

  public SearchOrdersRequestHandler(IReadRepository<Order> repository) => _repository = repository;

  public async Task<PaginationResponse<OrderDto>> Handle(SearchOrdersRequest request, CancellationToken
    cancellationToken)
  {
    var spec = new OrdersBySearchRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}