using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class CashRegisterArchivedOperationDto : CashRegisterOperationDto
{
}

public class SearchCashRegisterArchivedOperationsRequest : PaginationFilter,
  IRequest<PaginationResponse<CashRegisterArchivedOperationDto>>
{
  public Guid CashRegisterId { get; set; }
}

public class SearchCashRegisterArchivedOperationSpec : Specification<ArchivedPaymentOperation, CashRegisterArchivedOperationDto>
{
  public SearchCashRegisterArchivedOperationSpec(SearchCashRegisterArchivedOperationsRequest request)
    => Query.Where(a => a.CashRegisterId == request.CashRegisterId);
}

public class SearchCashRegisterArchivedOperationsHandler
  : IRequestHandler<SearchCashRegisterArchivedOperationsRequest, PaginationResponse<CashRegisterArchivedOperationDto>>
{
  private readonly IReadRepository<ArchivedPaymentOperation> _activeOpRepo;
  private readonly IStringLocalizer<SearchCashRegisterArchivedOperationsHandler> _t;

  public SearchCashRegisterArchivedOperationsHandler(IRepositoryWithEvents<CashRegister> repository,
    IStringLocalizer<SearchCashRegisterArchivedOperationsHandler> localizer,
    IReadRepository<ArchivedPaymentOperation> activeOpRepo)
  {
    _t = localizer;
    _activeOpRepo = activeOpRepo;
  }

  public Task<PaginationResponse<CashRegisterArchivedOperationDto>>
    Handle(SearchCashRegisterArchivedOperationsRequest request, CancellationToken cancellationToken)
    => _activeOpRepo.PaginatedListAsync(new SearchCashRegisterArchivedOperationSpec(request),
      request.PageNumber, request.PageSize, cancellationToken);
}