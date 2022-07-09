using FSH.WebApi.Domain.Operation;

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

  public CommitCashRegisterTransferHandler(IReadRepository<CashRegister> cashRegisterRepo,
    IStringLocalizer<TransferFromCashRegisterHandler> localizer, IRepositoryWithEvents<ActivePaymentOperation> repository)
  {
    _cashRegisterRepo = cashRegisterRepo;
    _t = localizer;
    _repository = repository;
  }

  public async Task<string> Handle(CommitCashRegisterTransferRequest request, CancellationToken cancellationToken)
  {
    var tr = await _repository.GetByIdAsync(request.TransferId, cancellationToken);
    if (tr.Type != PaymentOperationType.PendingIn || tr.PendingTransferId == null)
    {
      throw new InvalidOperationException(_t["Invalid transfer operation"]);
    }

    var pendingOut = await _repository.GetByIdAsync(tr.PendingTransferId, cancellationToken);
    if (pendingOut == null || pendingOut.Type != PaymentOperationType.PendingOut)
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
    await _repository.UpdateAsync(pendingOut, cancellationToken);

    sourceCr.CommitPendingOut(pendingOut);
    await _repository.UpdateAsync(tr, cancellationToken);

    return _t["Transfer committed"];
  }
}