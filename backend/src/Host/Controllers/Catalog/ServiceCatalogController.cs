using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

public class ServiceCatalogController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Search service catalog using available filters.", "")]
  [HasValidSubscriptionType(SubscriptionType.Standard)]
  public Task<PaginationResponse<ServiceCatalogDto>> SearchAsync(SearchServiceCatalogRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Create a new service catalog.", "")]
  public Task<Guid> CreateAsync(CreateServiceCatalogRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Update a service catalog.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateServiceCatalogRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Delete a service catalog.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteServiceCatalogRequest(id));
  }
}