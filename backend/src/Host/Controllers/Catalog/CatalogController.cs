using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

public sealed class CatalogController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Search service catalog using available filters.", "")]
  public Task<PaginationResponse<ServiceCatalogDto>> SearchAsync(SearchServiceCatalogRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.Search, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Search service catalog using available filters.", "")]
  public Task<ServiceCatalogDto> GetByIdAsync(Guid id)
  {
    return Mediator.Send(new GetServiceCatalogByIdRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Create a new service catalog.", "")]
  public Task<Guid> CreateAsync(CreateServiceCatalogRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("with-product-and-service")]
  [MustHavePermission(FSHAction.Create, FSHResource.ServiceCatalog)]
  [OpenApiOperation("Create a new service catalog item with new product and new service.", "")]
  public Task<Guid> CreateWithProductAndService(CreateServiceCatalogFromProductAndServiceRequest request)
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