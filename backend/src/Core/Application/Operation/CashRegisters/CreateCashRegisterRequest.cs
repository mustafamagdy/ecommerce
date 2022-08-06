using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class CreateCashRegisterRequest : IRequest<Guid>
{
  public Guid BranchId { get; set; }
  public string Name { get; set; }
  public string Color { get; set; }
}

public sealed class CashRegisterByNameSpec : Specification<CashRegister>, ISingleResultSpecification
{
  public CashRegisterByNameSpec(string name) => Query.Where(b => b.Name == name);
}

public class CreateCashRegisterRequestValidator : CustomValidator<CreateCashRegisterRequest>
{
  public CreateCashRegisterRequestValidator(IReadRepository<CashRegister> repository, IStringLocalizer<CreateCashRegisterRequestValidator> T)
  {
    RuleFor(p => p.Name)
      .NotEmpty()
      .MaximumLength(75)
      .MustAsync(async (name, ct) => await repository.GetBySpecAsync(new CashRegisterByNameSpec(name), ct) is null)
      .WithMessage((_, name) => T["Cash Register {0} already Exists.", name]);
  }
}

public class CreateCashRegisterRequestHandler : IRequestHandler<CreateCashRegisterRequest, Guid>
{
  private readonly IRepositoryWithEvents<CashRegister> _repository;
  private readonly IApplicationUnitOfWork _uow;

  public CreateCashRegisterRequestHandler(IRepositoryWithEvents<CashRegister> repository, IApplicationUnitOfWork uow)
  {
    _repository = repository;
    _uow = uow;
  }

  public async Task<Guid> Handle(CreateCashRegisterRequest request, CancellationToken cancellationToken)
  {
    var cr = new CashRegister(request.BranchId, request.Name, request.Color);
    await _repository.AddAsync(cr, cancellationToken);
    await _uow.CommitAsync(cancellationToken);

    return cr.Id;
  }
}