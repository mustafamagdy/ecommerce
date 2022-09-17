using Finbuckle.MultiTenant;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Multitenancy;

public class DeleteBranchRequest : IRequest
{
  public Guid Id { get; set; }
}

public class CashRegisterByBranchId : Specification<CashRegister>
{
  public CashRegisterByBranchId(Guid branchId) => Query.Where(a => a.BranchId == branchId);
}

public class DeleteBranchRequestHandler : IRequestHandler<DeleteBranchRequest>
{
  private readonly IRepository<Branch> _repository;
  private readonly IReadRepository<CashRegister> _cashRegisterRepo;
  private readonly IApplicationUnitOfWork _uow;
  private readonly IStringLocalizer _t;

  public DeleteBranchRequestHandler(IRepository<Branch> repository, IApplicationUnitOfWork uow,
    IReadRepository<CashRegister> cashRegisterRepo, IStringLocalizer<DeleteBranchRequestHandler> localizer)
  {
    _repository = repository;
    _uow = uow;
    _cashRegisterRepo = cashRegisterRepo;
    _t = localizer;
  }

  public async Task<Unit> Handle(DeleteBranchRequest request, CancellationToken cancellationToken)
  {
    var branch = await _repository.GetByIdAsync(request.Id, cancellationToken)
                 ?? throw new NotFoundException(_t["Branch {0} not found", request.Id]);

    var hasAnyCashRegister = await _cashRegisterRepo.AnyAsync(new CashRegisterByBranchId(request.Id), cancellationToken);
    if (hasAnyCashRegister)
    {
      throw new ConflictException(_t["Brand cannot be deleted as it's being used."]);
    }

    await _repository.DeleteAsync(branch, cancellationToken);
    await _uow.CommitAsync(cancellationToken);
    return Unit.Value;
  }
}