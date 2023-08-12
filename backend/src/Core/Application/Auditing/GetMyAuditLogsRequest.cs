using FSH.WebApi.Domain.Auditing;

namespace FSH.WebApi.Application.Auditing;

public class GetMyAuditLogsRequest : IRequest<List<AuditDto>>
{
}

public class UserAuditTrailSpec : Specification<Trail, AuditDto>
{
  public UserAuditTrailSpec(Guid userId) =>
    Query
      .Where(a => a.UserId == userId)
      .OrderByDescending(a => a.DateTime)
      .Take(250);
}

public class GetMyAuditLogsRequestHandler : IRequestHandler<GetMyAuditLogsRequest, List<AuditDto>>
{
  private readonly ICurrentUser _currentUser;
  private readonly IReadNonAggregateRepository<Trail> _trailRepo;


  public GetMyAuditLogsRequestHandler(ICurrentUser currentUser, IReadNonAggregateRepository<Trail> trailRepo)
  {
    _currentUser = currentUser;
    _trailRepo = trailRepo;
  }

  public Task<List<AuditDto>> Handle(GetMyAuditLogsRequest request, CancellationToken cancellationToken)
    => _trailRepo.ListAsync(new UserAuditTrailSpec(_currentUser.GetUserId()), cancellationToken);
}