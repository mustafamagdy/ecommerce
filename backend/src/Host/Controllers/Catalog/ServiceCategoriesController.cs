using FSH.WebApi.Application.Catalog.ServiceCategories;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

public class ServiceCategoriesController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.ServiceCategories)]
  [OpenApiOperation("Search service categories using available filters.", "")]
  [HasValidSubscriptionLevel(SubscriptionLevel.Basic)]
  public Task<PaginationResponse<ServiceCategoryDto>> SearchAsync(SearchServiceCategoriesRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.ServiceCategories)]
  [OpenApiOperation("Get service category details.", "")]
  public Task<ServiceCategoryDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetServiceCategoryRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.ServiceCategories)]
  [OpenApiOperation("Create a new service category.", "")]
  public Task<Guid> CreateAsync(CreateServiceCategoryRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.ServiceCategories)]
  [OpenApiOperation("Update a service category.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateServiceCategoryRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.ServiceCategories)]
  [OpenApiOperation("Delete a service category.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteServiceCategoryRequest(id));
  }
}