using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Orders;

namespace FSH.WebApi.Host.Controllers.Operation;

public class FinanceController : VersionedApiController
{
  [HttpPost("cash-register")]
  [MustHavePermission(FSHAction.Create, FSHResource.CashRegisters)]
  [OpenApiOperation("Create a cash register for a branch.", "")]
  public Task<Guid> CreateCashRegister(CreateCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("cash-register")]
  [MustHavePermission(FSHAction.Update, FSHResource.CashRegisters)]
  [OpenApiOperation("Update a cash register for a branch.", "")]
  public Task UpdateCashRegister(UpdateCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete("cash-register")]
  [MustHavePermission(FSHAction.Delete, FSHResource.CashRegisters)]
  [OpenApiOperation("Delete a cash register for a branch.", "")]
  public Task DeleteCashRegister(DeleteCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("cash-register/open")]
  [MustHavePermission(FSHAction.Open, FSHResource.CashRegisters)]
  [OpenApiOperation("Open a cash register for a branch.", "")]
  public Task OpenCashRegister(OpenCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("cash-register/close")]
  [MustHavePermission(FSHAction.Close, FSHResource.CashRegisters)]
  [OpenApiOperation("Close a cash register for a branch.", "")]
  public Task CloseCashRegister(CloseCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("cash-register/transfer")]
  [MustHavePermission(FSHAction.Transfer, FSHResource.CashRegisters)]
  [OpenApiOperation("Close a cash register for a branch.", "")]
  public Task TransferFromCashRegister(TransferFromCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("cash-register/approve-transfer")]
  [MustHavePermission(FSHAction.Approve, FSHResource.CashRegisters)]
  [OpenApiOperation("Close a cash register for a branch.", "")]
  public Task<string> CommitTransfer(CommitCashRegisterTransferRequest request)
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