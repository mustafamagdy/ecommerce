using Elsa.Services;
using FSH.WebApi.Application.Catalog.Brands;
using FSH.WebApi.Application.Dashboard;
using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Workflows;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Infrastructure.Identity;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Host.Controllers.DemoPermissions;

[AllowAnonymous]
public sealed class DemoController : VersionedApiController
{
  [HttpGet("test-workflow")]
  public async Task Test01([FromServices] IBuildsAndStartsWorkflow builder)
  {
    await builder.BuildAndStartWorkflowAsync<HelloWorldWorkflow>();
  }

  [HttpGet("{id:guid}")]
  [MustHavePermission(FSHAction.View, FSHResource.Brands)]
  [OpenApiOperation("Get brand details.", "")]
  public Task<BrandDto> GetAsync(Guid id)
  {
    return Mediator.Send(new GetBrandRequest(id));
  }

  [HttpPost]
  [MustHavePermission(FSHAction.Create, FSHResource.Brands)]
  [OpenApiOperation("Create a new brand.", "")]
  public Task<Guid> CreateAsync(CreateBrandRequest request)
  {
    return Mediator.Send(request);
  }

  [HttpDelete("{id:guid}")]
  [MustHavePermission(FSHAction.Delete, FSHResource.Brands)]
  [OpenApiOperation("Delete a brand.", "")]
  public Task<Guid> DeleteAsync(Guid id)
  {
    return Mediator.Send(new DeleteBrandRequest(id));
  }
}