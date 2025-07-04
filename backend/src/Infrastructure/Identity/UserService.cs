using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Caching;
using FSH.WebApi.Application.Common.Events;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.FileStorage;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Specification;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Domain.Identity;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FSH.WebApi.Infrastructure.Identity;

internal sealed partial class UserService : IUserService
{
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly RoleManager<ApplicationRole> _roleManager;
  private readonly ApplicationDbContext _db;
  private readonly IStringLocalizer _t;
  private readonly IJobService _jobService;
  private readonly IMailService _mailService;
  private readonly IdentitySettings _identitySettings;
  private readonly IEmailTemplateService _templateService;
  private readonly IFileStorageService _fileStorage;
  private readonly IEventPublisher _events;
  private readonly ICacheService _cache;
  private readonly ICacheKeyService _cacheKeys;
  private readonly ITenantInfo _currentTenant;

  public UserService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext db,
    IStringLocalizer<UserService> localizer,
    IJobService jobService,
    IMailService mailService,
    IEmailTemplateService templateService,
    IFileStorageService fileStorage,
    IEventPublisher events,
    ICacheService cache,
    ICacheKeyService cacheKeys,
    ITenantInfo currentTenant,
    IOptions<IdentitySettings> identitySettings)
  {
    _signInManager = signInManager;
    _userManager = userManager;
    _roleManager = roleManager;
    _db = db;
    _t = localizer;
    _jobService = jobService;
    _mailService = mailService;
    _templateService = templateService;
    _fileStorage = fileStorage;
    _events = events;
    _cache = cache;
    _cacheKeys = cacheKeys;
    _currentTenant = currentTenant;
    _identitySettings = identitySettings.Value;
  }

  public async Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
  {
    var spec = new EntitiesByPaginationFilterSpec<ApplicationUser>(filter);

    var users = await _userManager.Users
      .WithSpecification(spec)
      .ProjectToType<UserDetailsDto>()
      .ToListAsync(cancellationToken);

    int count = await _userManager.Users
      .CountAsync(cancellationToken);

    return new PaginationResponse<UserDetailsDto>(users, count, filter.PageNumber, filter.PageSize);
  }

  public async Task<bool> ExistsWithNameAsync(string name)
  {
    EnsureValidTenant();
    return await _userManager.FindByNameAsync(name) is not null;
  }

  public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null)
  {
    EnsureValidTenant();
    return await _userManager.FindByEmailAsync(email.Normalize()) is ApplicationUser user && user.Id != exceptId;
  }

  public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null)
  {
    EnsureValidTenant();
    return await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber) is ApplicationUser user && user.Id != exceptId;
  }

  private void EnsureValidTenant()
  {
    if (string.IsNullOrWhiteSpace(_currentTenant?.Id))
    {
      throw new UnauthorizedException(_t["Invalid Tenant."]);
    }
  }

  public async Task<PaginationResponse<UserDetailsDto>> GetListAsync(PaginationFilter filter, CancellationToken cancellationToken)
  {
    var spec = new EntitiesByPaginationFilterSpec<ApplicationUser>(filter);

    var users = await _db.Users
      .Include(a => a.UserRoles)
      .ThenInclude(a => a.Role)
      .WithSpecification(spec)
      .ProjectToType<UserDetailsDto>()
      .ToListAsync(cancellationToken);

    int count = await _userManager.Users
      .CountAsync(cancellationToken);

    return new PaginationResponse<UserDetailsDto>(users, count, filter.PageNumber, filter.PageSize);
  }

  public async Task<List<BasicUserDataDto>> GetListBasicDataAsync(CancellationToken cancellationToken)
  {
    var users = await _db.Users
      .Include(a => a.UserRoles)
      .ThenInclude(a => a.Role)
      .ProjectToType<BasicUserDataDto>()
      .ToListAsync(cancellationToken);
    return users;
  }

  public Task<int> GetCountAsync(CancellationToken cancellationToken) =>
    _userManager.Users.AsNoTracking().CountAsync(cancellationToken);

  public async Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken)
  {
    var user = (await _db.Users
        .Include(a => a.UserRoles)
        .ThenInclude(a => a.Role)
        .FirstOrDefaultAsync(a => a.Id == userId)
      ).Adapt<UserDetailsDto>();
    return user;
  }

  public async Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken)
  {
    var user = await _userManager.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);

    _ = user ?? throw new NotFoundException(_t["User Not Found."]);

    bool isAdmin = await _userManager.IsInRoleAsync(user, FSHRoles.Admin);
    if (isAdmin)
    {
      throw new ConflictException(_t["Administrators Profile's Status cannot be toggled"]);
    }

    user.Active = request.ActivateUser;

    await _userManager.UpdateAsync(user);

    await _events.PublishAsync(new ApplicationUserUpdatedEvent(user.Id));
  }
}