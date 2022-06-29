using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Multitenancy;

public class BranchController : VersionNeutralApiController
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
  public Task UpdateBranchAsync(UpdateBranchRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Branches)]
  [OpenApiOperation("Search for a branch.", "")]
  public Task<BranchDto> GetListAsync(SearchBranchRequest request)
  {
    return Mediator.Send(request);
  }
}