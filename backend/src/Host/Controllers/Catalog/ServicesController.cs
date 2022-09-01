using FSH.WebApi.Application.Catalog.Services;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

public class ServicesController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Services)]
  [OpenApiOperation("Search brands using available filters.", "")]
  public Task<PaginationResponse<ServiceDto>> SearchAsync(SearchServicesRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Services)]
  [OpenApiOperation("Get brand details.", "")]
  public Task<ServiceDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetServiceRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Services)]
  [OpenApiOperation("Create a new brand.", "")]
  public Task<Guid> CreateAsync(CreateServiceRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.Services)]
  [OpenApiOperation("Update a brand.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateServiceRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Services)]
  [OpenApiOperation("Delete a brand.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteServiceRequest(id));
  }
}