using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class PaymentOperationDto : IDto
{
  public DateTime DateTime { get; set; }
  public decimal Amount { get; set; }
  public PaymentOperationType Type { get; set; }
  public string PaymentMethodName { get; set; }
}

public class CashRegisterTransactionSearchRequest : PaginationFilter, IRequest<PaginationResponse<PaymentOperationDto>>
{
  public Guid Id { get; set; }
  public Range<DateTime> OperationDateTime { get; set; }
}

public sealed class CashRegisterTransactionsByIdSpec : EntitiesByPaginationFilterSpec<CashRegister, PaymentOperationDto>
{
  public CashRegisterTransactionsByIdSpec(CashRegisterTransactionSearchRequest request)
    : base(request) => Query
    .Include(a => a.ActiveOperations)
    .ThenInclude(a => a.PaymentMethod)
    .Include(a => a.ArchivedOperations)
    .ThenInclude(a => a.PaymentMethod)
    .Where(a => a.Id == request.Id)
    .Where(a => request.OperationDateTime.From == null
                || a.ActiveOperations.Any(x => x.DateTime >= request.OperationDateTime.From)
                || a.ArchivedOperations.Any(x => x.DateTime >= request.OperationDateTime.From))
    .Where(a => request.OperationDateTime.To == null
                || a.ActiveOperations.Any(x => x.DateTime <= request.OperationDateTime.To)
                || a.ArchivedOperations.Any(x => x.DateTime <= request.OperationDateTime.To));
}

public class CashRegisterTransactionSearchRequestValidator : CustomValidator<CashRegisterTransactionSearchRequest>
{
  public CashRegisterTransactionSearchRequestValidator(IReadRepository<CashRegister> repository) =>
    RuleFor(p => p.Id).NotEmpty();
}

public class CashRegisterTransactionSearchRequestHandler : IRequestHandler<CashRegisterTransactionSearchRequest, PaginationResponse<PaymentOperationDto>>
{
  private readonly IReadRepository<CashRegister> _repository;

  public CashRegisterTransactionSearchRequestHandler(IReadRepository<CashRegister> repository) => _repository =
    repository;

  public async Task<PaginationResponse<PaymentOperationDto>> Handle(CashRegisterTransactionSearchRequest request, CancellationToken cancellationToken)
  {
    var spec = new CashRegisterTransactionsByIdSpec(request);
    return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
  }
}