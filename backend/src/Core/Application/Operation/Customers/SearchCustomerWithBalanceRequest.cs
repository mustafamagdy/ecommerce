using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class CustomerWithBalanceDto : BasicCustomerDto
{
  public decimal TotalOrders { get; set; }
  public decimal DueAmount { get; set; }
}

public class SearchCustomerWithBalanceRequest : PaginationFilter, IRequest<PaginationResponse<CustomerWithBalanceDto>>
{
  public Range<decimal> Balance { get; set; } = new();
  public Guid? CustomerId { get; set; }
  public string? Name { get; set; }
  public string? PhoneNumber { get; set; }
}

public class SearchCustomerWithBalanceRequestSpec : EntitiesByPaginationFilterSpec<Customer, CustomerWithBalanceDto>
{
  public SearchCustomerWithBalanceRequestSpec(SearchCustomerWithBalanceRequest request)
    : base(request) =>
    Query
      .Where(a => request.CustomerId == null || a.Id == request.CustomerId)
      .Where(a => request.PhoneNumber == null || a.PhoneNumber == request.PhoneNumber)
      .Where(a => request.Name == null || a.Name == request.Name)
      .Where(a => request.Balance.From == null || a.DueAmount >= request.Balance.From)
      .Where(a => request.Balance.To == null || a.DueAmount <= request.Balance.To)
      .OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchCustomerWithBalanceRequestHandler : IRequestHandler<SearchCustomerWithBalanceRequest, PaginationResponse<CustomerWithBalanceDto>>
{
  private readonly IReadRepository<Customer> _repository;

  public SearchCustomerWithBalanceRequestHandler(IReadRepository<Customer> repository) => _repository = repository;

  public async Task<PaginationResponse<CustomerWithBalanceDto>> Handle(SearchCustomerWithBalanceRequest request, CancellationToken cancellationToken)
  {
    var spec = new SearchCustomerWithBalanceRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}