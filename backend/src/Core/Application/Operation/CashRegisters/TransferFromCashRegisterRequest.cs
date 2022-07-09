using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class TransferFromCashRegisterRequest : IRequest<string>
{
  public Guid SourceCashRegisterId { get; set; }
  public Guid DestCashRegisterId { get; set; }
  public decimal Amount { get; set; }
  public DateTime DateTime { get; set; }
}

public class TransferFromCashRegisterHandler : IRequestHandler<TransferFromCashRegisterRequest, string>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IRepository<ActivePaymentOperation> _activeOpRepo;
  private readonly IStringLocalizer<TransferFromCashRegisterHandler> _t;

  public TransferFromCashRegisterHandler(IRepositoryWithEvents<CashRegister> repository, IStringLocalizer<TransferFromCashRegisterHandler> localizer, IRepository<ActivePaymentOperation> activeOpRepo)
  {
    _repository = repository;
    _t = localizer;
    _activeOpRepo = activeOpRepo;
  }

  public async Task<string> Handle(TransferFromCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var sourceCr = await _repository.GetByIdAsync(request.SourceCashRegisterId, cancellationToken);
    if (sourceCr == null)
    {
      throw new NotFoundException(_t["Source cash register not found"]);
    }

    var destCr = await _repository.GetByIdAsync(request.SourceCashRegisterId, cancellationToken);
    if (destCr == null)
    {
      throw new NotFoundException(_t["Destination cash register not found"]);
    }

    if (!sourceCr.Opened)
    {
      throw new InvalidOperationException(_t["Source cash register is not opened for operations"]);
    }

    if (request.Amount > sourceCr.Balance)
    {
      throw new InvalidOperationException(_t["Not enough balance in the source cash register"]);
    }

    var (src, dest) = ActivePaymentOperation.CreateTransfer(request.SourceCashRegisterId, request.DestCashRegisterId,
      request.Amount, request.DateTime);

    await _activeOpRepo.AddAsync(src, cancellationToken);
    await _activeOpRepo.AddAsync(dest, cancellationToken);

    await _repository.UpdateAsync(sourceCr, cancellationToken);
    return _t["Cash register opened"];
  }
}
