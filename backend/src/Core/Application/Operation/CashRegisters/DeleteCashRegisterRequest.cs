using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class DeleteCashRegisterRequest : IRequest
{
  public Guid Id { get; set; }
}

public class DeleteCashRegisterRequestValidator : CustomValidator<DeleteCashRegisterRequest>
{
  public DeleteCashRegisterRequestValidator()
  {
    RuleFor(p => p.Id)
      .NotEmpty();
  }
}

public class PaymentOperationByCashRegisterIdSpec : Specification<ActivePaymentOperation>
{
  public PaymentOperationByCashRegisterIdSpec(Guid cashRegisterId) => Query.Where(a => a.CashRegisterId == cashRegisterId);
}

public class DeleteCashRegisterRequestHandler : IRequestHandler<DeleteCashRegisterRequest>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IStringLocalizer<DeleteCashRegisterRequestHandler> _t;
  private readonly IReadRepository<ActivePaymentOperation> _activeOpsRepo;
  private readonly IReadRepository<ArchivedPaymentOperation> _archiveOpsRepo;
  private readonly IApplicationUnitOfWork _uow;

  public DeleteCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository,
    IStringLocalizer<DeleteCashRegisterRequestHandler> localizer, IReadRepository<ActivePaymentOperation> activeOpsRepo,
    IReadRepository<ArchivedPaymentOperation> archiveOpsRepo, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _t = localizer;
    _activeOpsRepo = activeOpsRepo;
    _archiveOpsRepo = archiveOpsRepo;
    _uow = uow;
  }

  public async Task<Unit> Handle(DeleteCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var cr = await _repository.GetByIdAsync(request.Id, cancellationToken);
    if (cr == null)
    {
      throw new NotFoundException(_t["Cash register {0} is not found", request.Id]);
    }

    bool hasActiveOps = await _activeOpsRepo.AnyAsync(new PaymentOperationByCashRegisterIdSpec(request.Id), cancellationToken);
    bool hasArchivedOps = await _activeOpsRepo.AnyAsync(new PaymentOperationByCashRegisterIdSpec(request.Id), cancellationToken);
    if (hasActiveOps || hasArchivedOps)
    {
      throw new InvalidOperationException(_t["Cash register cannot be delete as it has operations"]);
    }

    await _repository.DeleteAsync(cr, cancellationToken);
    await _uow.CommitAsync(cancellationToken);

    return Unit.Value;
  }
}