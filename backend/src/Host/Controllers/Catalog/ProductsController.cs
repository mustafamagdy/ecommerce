using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Catalog.ServiceCatalogs;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Host.Controllers.Catalog;

[HasValidSubscription]
public sealed class ProductsController : VersionedApiController
{
  [HttpPost("search")]
  [MustHavePermission(FSHAction.Search, FSHResource.Products)]
  [OpenApiOperation("Search products using available filters.", "")]
  public Task<PaginationResponse<ProductDto>> SearchAsync(SearchProductsRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPost("not-in-service")]
  [MustHavePermission(FSHAction.Search, FSHResource.Products)]
  [OpenApiOperation("Get products not assigned to service by service-id.", "")]
  public Task<PaginationResponse<ProductDto>> SearchAsync(GetProductsNotInServiceIdRequest request)
  {
    return Mediator.Send(request);
  }


  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Products)]
  [OpenApiOperation("Get product details.", "")]
  public Task<ProductDetailsDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetProductRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Products)]
  [OpenApiOperation("Create a new product.", "")]
  public Task<Guid> CreateAsync(CreateProductRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpPut("{id:guid}")]
  [MustHavePermission(FSHAction.Update, FSHResource.Products)]
  [OpenApiOperation("Update a product.", "")]
  public async Task<ActionResult<Guid>> UpdateAsync(UpdateProductRequest request, Guid id)
  {
    return id != request.Id
      ? BadRequest()
      : Ok(await Mediator.Send(request));
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Products)]
  [OpenApiOperation("Delete a product.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteProductRequest(id));
  }

  [HttpPost("export")]
  [MustHavePermission(FSHAction.Export, FSHResource.Products)]
  [OpenApiOperation("Export a products.", "")]
  public async Task<FileResult> ExportAsync(ExportProductsRequest filter)
  {
    var result = await Mediator.Send(filter);
    return File(result, "application/octet-stream", "ProductExports");
  }
}