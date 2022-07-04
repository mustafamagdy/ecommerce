using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class SearchBasicCashRegistersRequest : PaginationFilter, IRequest<PaginationResponse<BasicCashRegisterDto>>
{
}

public class BasicCashRegistersBySearchRequestSpec : EntitiesByPaginationFilterSpec<CashRegister, BasicCashRegisterDto>
{
  public BasicCashRegistersBySearchRequestSpec(SearchBasicCashRegistersRequest request)
    : base(request) =>
    Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchCashRegistersRequest : PaginationFilter, IRequest<PaginationResponse<CashRegisterWithBalanceDto>>
{
}

public class CashRegistersBySearchRequestSpec : EntitiesByPaginationFilterSpec<CashRegister, CashRegisterWithBalanceDto>
{
  public CashRegistersBySearchRequestSpec(SearchCashRegistersRequest request)
    : base(request) =>
    Query.OrderBy(c => c.Name, !request.HasOrderBy());
}

public class SearchBasicCashRegistersRequestHandler : IRequestHandler<SearchBasicCashRegistersRequest, PaginationResponse<BasicCashRegisterDto>>
{
  private readonly IReadRepository<CashRegister> _repository;

  public SearchBasicCashRegistersRequestHandler(IReadRepository<CashRegister> repository) => _repository = repository;

  public async Task<PaginationResponse<BasicCashRegisterDto>> Handle(SearchBasicCashRegistersRequest request, CancellationToken cancellationToken)
  {
    var spec = new BasicCashRegistersBySearchRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}

public class SearchCashRegistersRequestHandler : IRequestHandler<SearchCashRegistersRequest, PaginationResponse<CashRegisterWithBalanceDto>>
{
  private readonly IReadRepository<CashRegister> _repository;

  public SearchCashRegistersRequestHandler(IReadRepository<CashRegister> repository) => _repository = repository;

  public async Task<PaginationResponse<CashRegisterWithBalanceDto>> Handle(SearchCashRegistersRequest request, CancellationToken cancellationToken)
  {
    var spec = new CashRegistersBySearchRequestSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}