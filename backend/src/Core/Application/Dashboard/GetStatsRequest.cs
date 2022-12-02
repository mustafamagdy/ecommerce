using FSH.WebApi.Application.Identity.Roles;
using FSH.WebApi.Application.Identity.Users;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Dashboard;

public class GetStatsRequest : IRequest<StatsDto>
{
}

public class GetStatsRequestHandler : IRequestHandler<GetStatsRequest, StatsDto>
{
  private readonly ISystemTime _time;
  private readonly IUserService _userService;
  private readonly IRoleService _roleService;
  private readonly IReadRepository<ServiceCatalog> _serviceCatalogRepo;
  private readonly IReadRepository<Branch> _branchRepo;
  private readonly IReadRepository<Order> _orderRepo;
  private readonly IStringLocalizer _t;

  public GetStatsRequestHandler(IUserService userService, IRoleService roleService,
    IStringLocalizer<GetStatsRequestHandler> localizer, IReadRepository<ServiceCatalog> serviceCatalogRepo,
    IReadRepository<Branch> branchRepo, ISystemTime time, IReadRepository<Order> orderRepo)
  {
    _userService = userService;
    _roleService = roleService;
    _t = localizer;
    _serviceCatalogRepo = serviceCatalogRepo;
    _branchRepo = branchRepo;
    _time = time;
    _orderRepo = orderRepo;
  }

  public async Task<StatsDto> Handle(GetStatsRequest request, CancellationToken cancellationToken)
  {
    var stats = new StatsDto
    {
      ServiceCatalogCount = await _serviceCatalogRepo.CountAsync(cancellationToken).ConfigureAwait(false),
      BranchCount = await _branchRepo.CountAsync(cancellationToken).ConfigureAwait(false),
      OrderCount = await _orderRepo.CountAsync(cancellationToken).ConfigureAwait(false),
      UserCount = await _userService.GetCountAsync(cancellationToken).ConfigureAwait(false),
      RoleCount = await _roleService.GetCountAsync(cancellationToken).ConfigureAwait(false)
    };

    int selectedYear = _time.Now .Year;
    double[] ordersCountFigure = new double[13];
    double[] ordersTotalFigure = new double[13];
    for (int month = 1; month <= 12; month++)
    {
      var filterStartDate = new DateTime(selectedYear, month, 01).ToUniversalTime();

      // Monthly Based
      var filterEndDate = new DateTime(selectedYear, month, DateTime.DaysInMonth(selectedYear, month), 23, 59, 59).ToUniversalTime();

      var orderSpec = new AuditableEntitiesByCreatedOnBetweenSpec<Order>(filterStartDate, filterEndDate);

      ordersCountFigure[month - 1] = await _orderRepo.CountAsync(orderSpec, cancellationToken).ConfigureAwait(false);
      ordersTotalFigure[month - 1] = (double)(await _orderRepo.SumAsync(orderSpec, a => a.TotalAmount, cancellationToken).ConfigureAwait(false) ?? 0);
    }

    stats.DataEnterBarChart.Add(new ChartSeries { Name = _t["Order Count"], Data = ordersCountFigure });
    stats.DataEnterBarChart.Add(new ChartSeries { Name = _t["Order Total Amount"], Data = ordersTotalFigure });

    return stats;
  }
}