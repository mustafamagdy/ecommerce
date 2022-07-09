using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public abstract class CashRegisterRequest : IRequest<string>
{
  public Guid Id { get; set; }
  public string Notes { get; set; }
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

  public OpenCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository, IStringLocalizer<OpenCashRegisterRequestHandler> localizer)
  {
    _repository = repository;
    _t = localizer;
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
    return _t["Cash register opened"];
  }
}

public class CloseCashRegisterRequestHandler : IRequestHandler<CloseCashRegisterRequest, string>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IStringLocalizer<CloseCashRegisterRequestHandler> _t;

  public CloseCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository, IStringLocalizer<CloseCashRegisterRequestHandler> localizer)
  {
    _repository = repository;
    _t = localizer;
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
    return _t["Cash register closed"];
  }
}