using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Customers;

public class SearchCustomersRequest : PaginationFilter, IRequest<PaginationResponse<BasicCustomerDto>>
{
  public string? Name { get; set; }
  public string? PhoneNumber { get; set; }
}

public class CustomersBySearchRequestSpec : EntitiesByPaginationFilterSpec<Customer, BasicCustomerDto>
{
  public CustomersBySearchRequestSpec(SearchCustomersRequest request)
    : base(request) =>
    Query
      .Where(a => request.PhoneNumber == null || a.PhoneNumber == request.PhoneNumber)
      .Where(a => request.Name == null || a.Name == request.Name)
      .OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchCustomersRequestHandler : IRequestHandler<SearchCustomersRequest, PaginationResponse<BasicCustomerDto>>
{
  private readonly IReadRepository<Customer> _repository;

  public SearchCustomersRequestHandler(IReadRepository<Customer> repository) => _repository = repository;

  public async Task<PaginationResponse<BasicCustomerDto>> Handle(SearchCustomersRequest request, CancellationToken cancellationToken)
  {
    var spec = new CustomersBySearchRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}