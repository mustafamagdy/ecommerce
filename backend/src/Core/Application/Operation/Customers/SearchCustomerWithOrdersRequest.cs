using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class CustomerWithOrdersDto : BasicCustomerDto
{
  public decimal TotalOrders { get; set; }
  public decimal DueAmount { get; set; }

  public IReadOnlyCollection<BasicOrderDto> Orders { get; set; }
}

public class SearchCustomerWithOrdersRequest : PaginationFilter, IRequest<PaginationResponse<CustomerWithOrdersDto>>
{
  public Range<decimal> Balance { get; set; } = new();
  public Guid? CustomerId { get; set; }
  public string? Name { get; set; }
  public string? PhoneNumber { get; set; }
}

public class SearchCustomerWithOrdersRequestSpec : EntitiesByPaginationFilterSpec<Customer, CustomerWithOrdersDto>
{
  public SearchCustomerWithOrdersRequestSpec(SearchCustomerWithOrdersRequest request)
    : base(request) =>
    Query
      .Include(a => a.Orders)
      .Where(a => request.CustomerId == null || a.Id == request.CustomerId)
      .Where(a => request.PhoneNumber == null || a.PhoneNumber == request.PhoneNumber)
      .Where(a => request.Name == null || a.Name == request.Name)
      .Where(a => request.Balance.From == null || a.DueAmount >= request.Balance.From)
      .Where(a => request.Balance.To == null || a.DueAmount <= request.Balance.To)
      .OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchCustomerWithOrdersRequestHandler : IRequestHandler<SearchCustomerWithOrdersRequest, PaginationResponse<CustomerWithOrdersDto>>
{
  private readonly IReadRepository<Customer> _repository;

  public SearchCustomerWithOrdersRequestHandler(IReadRepository<Customer> repository) => _repository = repository;

  public async Task<PaginationResponse<CustomerWithOrdersDto>> Handle(SearchCustomerWithOrdersRequest request, CancellationToken cancellationToken)
  {
    var spec = new SearchCustomerWithOrdersRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}