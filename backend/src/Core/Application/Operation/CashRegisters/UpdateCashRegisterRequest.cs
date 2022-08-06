using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class UpdateCashRegisterRequest : IRequest
{
  public Guid Id { get; set; }
  public string Color { get; set; }
}

public class UpdateCashRegisterRequestValidator : CustomValidator<UpdateCashRegisterRequest>
{
  public UpdateCashRegisterRequestValidator()
  {
    RuleFor(p => p.Id)
      .NotEmpty();
  }
}

public class UpdateCashRegisterRequestHandler : IRequestHandler<UpdateCashRegisterRequest>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IStringLocalizer<UpdateCashRegisterRequestHandler> _t;
  private readonly IApplicationUnitOfWork _uow;

  public UpdateCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository,
    IStringLocalizer<UpdateCashRegisterRequestHandler> localizer, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _t = localizer;
    _uow = uow;
  }

  public async Task<Unit> Handle(UpdateCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var cr = await _repository.GetByIdAsync(request.Id, cancellationToken);
    if (cr == null)
    {
      throw new NotFoundException(_t["Cash register {0} is not found", request.Id]);
    }

    await _repository.UpdateAsync(cr, cancellationToken);
    await _uow.CommitAsync(cancellationToken);

    return Unit.Value;
  }
}