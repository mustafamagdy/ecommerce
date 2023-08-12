using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class GetCashRegisterWithBalanceRequest : IRequest<CashRegisterWithBalanceDto>
{
  public Guid Id { get; set; }

  public GetCashRegisterWithBalanceRequest(Guid id) => Id = id;
}

public class CashRegisterWithBalanceByIdSpec : Specification<CashRegister, CashRegisterWithBalanceDto>, ISingleResultSpecification
{
  public CashRegisterWithBalanceByIdSpec(Guid id) =>
    Query.Where(p => p.Id == id);
}

public class GetCashRegisterWithBalanceRequestHandler : IRequestHandler<GetCashRegisterWithBalanceRequest, CashRegisterWithBalanceDto>
{
  private readonly IRepository<CashRegister> _repository;
  private readonly IStringLocalizer _t;

  public GetCashRegisterWithBalanceRequestHandler(IRepository<CashRegister> repository, IStringLocalizer<GetCashRegisterWithBalanceRequestHandler> localizer) => (_repository, _t) = (repository, localizer);

  public async Task<CashRegisterWithBalanceDto> Handle(GetCashRegisterWithBalanceRequest request, CancellationToken cancellationToken) =>
    await _repository.FirstOrDefaultAsync(new CashRegisterWithBalanceByIdSpec(request.Id), cancellationToken)
    ?? throw new NotFoundException(_t["Cash Register {0} Not Found.", request.Id]);
}