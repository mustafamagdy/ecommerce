using FSH.WebApi.Application.Catalog.Categories;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

[HasValidSubscription]
public sealed class CategoriesController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Categories)]
  [OpenApiOperation("Search categories using available filters.", "")]
  public Task<PaginationResponse<CategoryDto>> SearchAsync(SearchCategoriesRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Categories)]
  [OpenApiOperation("Get product details.", "")]
  public Task<CategoryDetailsDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetCategoryRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Categories)]
  [OpenApiOperation("Create a new product.", "")]
  public Task<Guid> CreateAsync(CreateCategoryRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.Categories)]
  [OpenApiOperation("Update a product.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateCategoryRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Categories)]
  [OpenApiOperation("Delete a product.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteCategoryRequest(id));
  }
}