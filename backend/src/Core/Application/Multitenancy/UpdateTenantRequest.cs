using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Application.Multitenancy.EventHandlers;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Multitenancy;
using FSH.WebApi.Shared.Persistence;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Application.Multitenancy;

public class UpdateTenantRequest : IRequest<ViewTenantInfoDto>
{
  public string Id { get; set; }
  public string? Name { get; set; }
  public string? AdminEmail { get; set; }
  public string? PhoneNumber { get; set; }
  public string? VatNo { get; set; }
  public string? Email { get; set; }
  public string? Address { get; set; }
  public string? AdminName { get; set; }
  public string? AdminPhoneNumber { get; set; }
  public string? TechSupportUserId { get; set; }
}

public class UpdateTenantRequestValidator : CustomValidator<UpdateTenantRequest>
{
  public UpdateTenantRequestValidator()
  {
    RuleFor(a => a.Id).Must(a => MultitenancyConstants.RootTenant.Id != a)
      .WithMessage("Root tenant cannot be updated");
  }
}

public class UpdateTenantRequestHandler : IRequestHandler<UpdateTenantRequest, ViewTenantInfoDto>
{
  private readonly ITenantUnitOfWork _uow;
  private readonly IReadNonAggregateRepository<FSHTenantInfo> _tenantRepo;
  private readonly UserManager<ApplicationUser> _userManager;

  public UpdateTenantRequestHandler(ITenantUnitOfWork uow, IReadNonAggregateRepository<FSHTenantInfo> tenantRepo,
    UserManager<ApplicationUser> userManager)
  {
    _uow = uow;
    _tenantRepo = tenantRepo;
    _userManager = userManager;
  }

  public async Task<ViewTenantInfoDto> Handle(UpdateTenantRequest request, CancellationToken cancellationToken)
  {
    var tenant = await _tenantRepo.GetByIdAsync(request.Id, cancellationToken)
                 ?? throw new NotFoundException($"Tenant {request.Id} not found");

    tenant.Update(request.Name, request.AdminEmail, request.PhoneNumber, request.VatNo, request.Email,
      request.Address, request.AdminName, request.AdminPhoneNumber, request.TechSupportUserId);

    await _uow.CommitAsync(cancellationToken);
    var dto = tenant.Adapt<ViewTenantInfoDto>();
    if (!string.IsNullOrEmpty(dto.TechSupportUserId))
    {
      var supportUser = (await _userManager.FindByIdAsync(dto.TechSupportUserId) ??
                         throw new NotFoundException("Support user not found"))
        .Adapt<BasicUserDataDto>();
      dto.TechSupportName = supportUser.FullName;
    }

    return dto;
  }
}