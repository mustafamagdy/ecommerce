using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public abstract class CashRegisterRequest : IRequest<string>
{
  public Guid Id { get; set; }
  public string? Notes { get; set; }
}

public class OpenCashRegisterRequest : CashRegisterRequest
{
}

public class CloseCashRegisterRequest : CashRegisterRequest
{
}

public class OpenCashRegisterRequestHandler : IRequestHandler<OpenCashRegisterRequest, string>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IStringLocalizer<OpenCashRegisterRequestHandler> _t;
  private readonly IApplicationUnitOfWork _uow;

  public OpenCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository, IStringLocalizer<OpenCashRegisterRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _t = localizer;
    _uow = uow;
  }

  public async Task<string> Handle(OpenCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var cr = await _repository.GetByIdAsync(request.Id, cancellationToken);
    if (cr == null)
    {
      throw new NotFoundException(_t["Cash Register not found"]);
    }

    cr.Open();

    await _repository.UpdateAsync(cr, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return _t["Cash register opened"];
  }
}

public class CloseCashRegisterRequestHandler : IRequestHandler<CloseCashRegisterRequest, string>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IStringLocalizer<CloseCashRegisterRequestHandler> _t;
  private readonly IApplicationUnitOfWork _uow;

  public CloseCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository, IStringLocalizer<CloseCashRegisterRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _t = localizer;
    _uow = uow;
  }

  public async Task<string> Handle(CloseCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var cr = await _repository.GetByIdAsync(request.Id, cancellationToken);
    if (cr == null)
    {
      throw new NotFoundException(_t["Cash Register not found"]);
    }

    cr.Close();

    await _repository.UpdateAsync(cr, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return _t["Cash register closed"];
  }
}