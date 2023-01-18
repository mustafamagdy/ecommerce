using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class CommitCashRegisterTransferRequest : IRequest<string>
{
  public Guid TransferId { get; set; }
}

public class CommitCashRegisterTransferHandler : IRequestHandler<CommitCashRegisterTransferRequest, string>
{
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;
  private readonly IRepositoryWithEvents<ActivePaymentOperation> _repository;
  private readonly IStringLocalizer<TransferFromCashRegisterHandler> _t;
  private readonly IApplicationUnitOfWork _uow;

  public CommitCashRegisterTransferHandler(IReadRepository<CashRegister> cashRegisterRepo,
    IStringLocalizer<TransferFromCashRegisterHandler> localizer, IRepositoryWithEvents<ActivePaymentOperation> repository, IApplicationUnitOfWork uow)
  {
    _cashRegisterRepo = cashRegisterRepo;
    _t = localizer;
    _repository = repository;
    _uow = uow;
  }

  public async Task<string> Handle(CommitCashRegisterTransferRequest request, CancellationToken cancellationToken)
  {
    var tr = await _repository.GetByIdAsync(request.TransferId, cancellationToken)
             ?? throw new NotFoundException($"Transfer {request.TransferId} operation not found ");

    if (tr.OperationType != PaymentOperationType.PendingIn || tr.PendingTransferId == null)
    {
      throw new InvalidOperationException(_t["Invalid transfer operation"]);
    }

    var pendingOut = await _repository.GetByIdAsync(tr.PendingTransferId!, cancellationToken)
                     ?? throw new NotFoundException($"Pending transfer operation {tr.PendingTransferId} not found");

    if (pendingOut.OperationType != PaymentOperationType.PendingOut)
    {
      throw new InvalidOperationException(_t["Invalid transfer operation"]);
    }

    var sourceCr = await _cashRegisterRepo.GetByIdAsync(pendingOut.CashRegisterId, cancellationToken);
    if (sourceCr == null)
    {
      throw new NotFoundException(_t["Source cash register not found"]);
    }

    var destCr = await _cashRegisterRepo.GetByIdAsync(tr.CashRegisterId, cancellationToken);
    if (destCr == null)
    {
      throw new NotFoundException(_t["Destination cash register not found"]);
    }

    if (!destCr.Opened)
    {
      throw new InvalidOperationException(_t["cash register is not opened for operations"]);
    }

    destCr.AcceptPendingIn(tr);
    sourceCr.CommitPendingOut(pendingOut);

    await _uow.CommitAsync(cancellationToken);
    return _t["Transfer committed"];
  }
}