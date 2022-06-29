using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Orders;

namespace FSH.WebApi.Host.Controllers.Operation;

public class FinanceController : VersionNeutralApiController
{
  [HttpPost("cash-register")]
  [MustHavePermission(FSHAction.Create, FSHResource.CashRegisters)]
  [OpenApiOperation("Create a cash register for a branch.", "")]
  public Task<Guid> CreateBranchAsync(CreateCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("search-basic")]
  [MustHavePermission(FSHAction.ViewBasic, FSHResource.CashRegisters)]
  [OpenApiOperation("Search cash register basic info.", "")]
  public Task<PaginationResponse<BasicCashRegisterDto>> GetListAsync(SearchBasicCashRegistersRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("search-details")]
  [MustHavePermission(FSHAction.Search, FSHResource.CashRegisters)]
  [OpenApiOperation("Search cash register basic info.", "")]
  public Task<PaginationResponse<CashRegisterWithBalanceDto>> GetListAsync(SearchCashRegistersRequest request)
  {
    return Mediator.Send(request);
  }
}