using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Multitenancy;

public sealed class BranchController : VersionedApiController
{
  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Branches)]
  [OpenApiOperation("Create a branch for the current tenant.", "")]
  public Task<Guid> CreateBranchAsync(CreateBranchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut]
  [MustHavePermission(FSHAction.Update, FSHResource.Branches)]
  [OpenApiOperation("Update a branch for the current tenant.", "")]
  public Task<BranchDto> UpdateBranchAsync(UpdateBranchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Branches)]
  [OpenApiOperation("Delete a branch for the current tenant.", "")]
  public Task DeleteBranchAsync(Guid id)
  {
    return Mediator.Send(new DeleteBranchRequest(id));
  }

  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Branches)]
  [OpenApiOperation("Search for a branch.", "")]
  public Task<PaginationResponse<BranchDto>> GetListAsync(SearchBranchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("activate")]
  [MustHavePermission(FSHAction.Activate, FSHResource.Branches)]
  [OpenApiOperation("Activate a branch.", "")]
  public Task Activate(ActivateBranchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("deactivate")]
  [MustHavePermission(FSHAction.Deactivate, FSHResource.Branches)]
  [OpenApiOperation("Deactivate a branch.", "")]
  public Task Deactivate(DeactivateBranchRequest request)
  {
    return Mediator.Send(request);
  }
}