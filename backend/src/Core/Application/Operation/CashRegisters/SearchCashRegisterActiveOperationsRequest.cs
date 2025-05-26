using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class CashRegisterActiveOperationDto : CashRegisterOperationDto
{
}

public class SearchCashRegisterActiveOperationsRequest : PaginationFilter,
  IRequest<PaginationResponse<CashRegisterActiveOperationDto>>
{
  public Guid CashRegisterId { get; set; }
}

public class SearchCashRegisterActiveOperationSpec : Specification<ActivePaymentOperation, CashRegisterActiveOperationDto>
{
  public SearchCashRegisterActiveOperationSpec(SearchCashRegisterActiveOperationsRequest request)
    => Query.Where(a => a.CashRegisterId == request.CashRegisterId);
}

public class SearchCashRegisterActiveOperationsHandler
  : IRequestHandler<SearchCashRegisterActiveOperationsRequest, PaginationResponse<CashRegisterActiveOperationDto>>
{
  private readonly IReadRepository<ActivePaymentOperation> _activeOpRepo;
  private readonly IStringLocalizer<SearchCashRegisterActiveOperationsHandler> _t;

  public SearchCashRegisterActiveOperationsHandler(IRepositoryWithEvents<CashRegister> repository,
    IStringLocalizer<SearchCashRegisterActiveOperationsHandler> localizer,
    IReadRepository<ActivePaymentOperation> activeOpRepo)
  {
    _t = localizer;
    _activeOpRepo = activeOpRepo;
  }

  public Task<PaginationResponse<CashRegisterActiveOperationDto>> Handle(SearchCashRegisterActiveOperationsRequest request,
    CancellationToken cancellationToken)
  {
    return _activeOpRepo.PaginatedListAsync(new SearchCashRegisterActiveOperationSpec(request),
      request.PageNumber, request.PageSize, cancellationToken);
  }
}