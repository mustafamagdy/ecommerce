using FSH.WebApi.Application.Operation.Payments;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class TransferFromCashRegisterRequest : IRequest<Guid>
{
  public Guid SourceCashRegisterId { get; set; }
  public Guid DestCashRegisterId { get; set; }
  public decimal Amount { get; set; }
  public DateTime DateTime { get; set; }
}

public class TransferFromCashRegisterHandler : IRequestHandler<TransferFromCashRegisterRequest, Guid>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
  private readonly IStringLocalizer<TransferFromCashRegisterHandler> _t;
  private readonly IApplicationUnitOfWork _uow;
  public TransferFromCashRegisterHandler(IRepositoryWithEvents<CashRegister> repository,
    IStringLocalizer<TransferFromCashRegisterHandler> localizer, IApplicationUnitOfWork uow, IReadRepository<PaymentMethod> paymentMethodRepo)
  {
    _repository = repository;
    _t = localizer;
    _uow = uow;
    _paymentMethodRepo = paymentMethodRepo;
  }

  public async Task<Guid> Handle(TransferFromCashRegisterRequest request, CancellationToken cancellationToken)
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

    var cashPaymentMethod = await _paymentMethodRepo.FirstOrDefaultAsync(new GetDefaultCashPaymentMethodSpec(), cancellationToken);
    if (cashPaymentMethod is null)
    {
      throw new NotFoundException(_t["Cash payment method not configured"]);
    }

    var (src, dest) = ActivePaymentOperation.CreateTransfer(request.SourceCashRegisterId, request.DestCashRegisterId,
      request.Amount, request.DateTime, cashPaymentMethod.Id);

    sourceCr.AddOperation(src);
    destCr.AddOperation(dest);

    await _uow.CommitAsync(cancellationToken);
    return dest.Id;
  }
}