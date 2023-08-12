namespace FSH.WebApi.Application.Catalog.Services;

public class GetServiceRequest : IRequest<ServiceDto>
{
  public Guid Id { get; set; }

  public GetServiceRequest(Guid id) => Id = id;
}

public class ServiceByIdSpec : Specification<Service, ServiceDto>, ISingleResultSpecification
{
  public ServiceByIdSpec(Guid id) =>
    Query.Where(p => p.Id == id);
}

public class GetServiceRequestHandler : IRequestHandler<GetServiceRequest, ServiceDto>
{
  private readonly IRepository<Service> _repository;
  private readonly IStringLocalizer _t;

  public GetServiceRequestHandler(IRepository<Service> repository, IStringLocalizer<GetServiceRequestHandler>
    localizer) =>
    (_repository, _t) = (repository, localizer);

  public async Task<ServiceDto> Handle(GetServiceRequest request, CancellationToken cancellationToken)=>
       await _repository.FirstOrDefaultAsync(new ServiceByIdSpec(request.Id), cancellationToken)
          ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);
}