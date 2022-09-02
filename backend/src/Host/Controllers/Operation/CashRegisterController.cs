using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Application.Operation.Orders;

namespace FSH.WebApi.Host.Controllers.Operation;

public class CashRegisterController : VersionedApiController
{
  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.CashRegisters)]
  [OpenApiOperation("Get cash register basic data with status.", "")]
  public Task<BasicCashRegisterDto> GetCashRegister(Guid id)
  {
    return Mediator.Send(new GetCashRegisterRequest(id));
  }

  [HttpGet("{id:guid}/with-balance")]
  [MustHavePermission(FSHAction.View, FSHResource.CashRegisters)]
  [OpenApiOperation("Get cash register basic data with status and balance.", "")]
  public Task<CashRegisterWithBalanceDto> GetCashRegisterWithBalance(Guid id)
  {
    return Mediator.Send(new GetCashRegisterWithBalanceRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.CashRegisters)]
  [OpenApiOperation("Create a cash register for a branch.", "")]
  public Task<Guid> CreateCashRegister(CreateCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut]
  [MustHavePermission(FSHAction.Update, FSHResource.CashRegisters)]
  [OpenApiOperation("Update a cash register for a branch.", "")]
  public Task UpdateCashRegister(UpdateCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete]
  [MustHavePermission(FSHAction.Delete, FSHResource.CashRegisters)]
  [OpenApiOperation("Delete a cash register for a branch.", "")]
  public Task DeleteCashRegister(DeleteCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("open")]
  [MustHavePermission(FSHAction.Open, FSHResource.CashRegisters)]
  [OpenApiOperation("Open a cash register for a branch.", "")]
  public Task OpenCashRegister(OpenCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("close")]
  [MustHavePermission(FSHAction.Close, FSHResource.CashRegisters)]
  [OpenApiOperation("Close a cash register for a branch.", "")]
  public Task CloseCashRegister(CloseCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("transfer")]
  [MustHavePermission(FSHAction.Transfer, FSHResource.CashRegisters)]
  [OpenApiOperation("Transfer between cash registers.", "")]
  public Task<Guid> TransferFromCashRegister(TransferFromCashRegisterRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("approve-transfer")]
  [MustHavePermission(FSHAction.Approve, FSHResource.CashRegisters)]
  [OpenApiOperation("Approve a transfer for a cash register", "")]
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
  [OpenApiOperation("Search cash register basic info and balance.", "")]
  public Task<PaginationResponse<CashRegisterWithBalanceDto>> GetListAsync(SearchCashRegistersRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("search-active-operations")]
  [MustHavePermission(FSHAction.Search, FSHResource.CashRegisters)]
  [OpenApiOperation("Search cash register active operations.", "")]
  public Task<PaginationResponse<CashRegisterActiveOperationDto>> GetActiveOperations(SearchCashRegisterActiveOperationsRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("search-archived-operations")]
  [MustHavePermission(FSHAction.Search, FSHResource.CashRegisters)]
  [OpenApiOperation("Search cash register archived operations.", "")]
  public Task<PaginationResponse<CashRegisterArchivedOperationDto>> GetArchivedOperations(SearchCashRegisterArchivedOperationsRequest request)
  {
    return Mediator.Send(request);
  }
}