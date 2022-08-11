using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public class GetCashRegisterRequest : IRequest<BasicCashRegisterDto>
{
  public Guid Id { get; set; }

  public GetCashRegisterRequest(Guid id) => Id = id;
}

public class CashRegisterByIdSpec : Specification<CashRegister, BasicCashRegisterDto>, ISingleResultSpecification
{
  public CashRegisterByIdSpec(Guid id) =>
    Query.Where(p => p.Id == id);
}

public class GetCashRegisterRequestHandler : IRequestHandler<GetCashRegisterRequest, BasicCashRegisterDto>
{
  private readonly IRepository<CashRegister> _repository;
  private readonly IStringLocalizer _t;

  public GetCashRegisterRequestHandler(IRepository<CashRegister> repository, IStringLocalizer<GetCashRegisterRequestHandler> localizer) => (_repository, _t) = (repository, localizer);

  public async Task<BasicCashRegisterDto> Handle(GetCashRegisterRequest request, CancellationToken cancellationToken) =>
    await _repository.FirstOrDefaultAsync(new CashRegisterByIdSpec(request.Id), cancellationToken)
    ?? throw new NotFoundException(_t["Cash Register {0} Not Found.", request.Id]);
}