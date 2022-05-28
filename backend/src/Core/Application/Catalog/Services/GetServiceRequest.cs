﻿namespace FSH.WebApi.Application.Catalog.Services;

public class GetServiceRequest : IRequest<ServiceDto>
{
    public Guid Id { get; set; }

    public GetServiceRequest(Guid id) => Id = id;
}

public class ServiceByIdSpec : Specification<ServiceCatalog, ServiceDto>, ISingleResultSpecification
{
    public ServiceByIdSpec(Guid id) =>
        Query.Where(p => p.Id == id);
}

public class GetServiceRequestHandler : IRequestHandler<GetServiceRequest, ServiceDto>
{
    private readonly IRepository<ServiceCatalog> _repository;
    private readonly IStringLocalizer _t;

    public GetServiceRequestHandler(IRepository<ServiceCatalog> repository, IStringLocalizer<GetServiceRequestHandler>
    localizer) =>
    (_repository, _t) = (repository, localizer);

    public async Task<ServiceDto> Handle(GetServiceRequest request, CancellationToken cancellationToken) =>
        await _repository.GetBySpecAsync(
            (ISpecification<ServiceCatalog, ServiceDto>)new ServiceByIdSpec(request.Id), cancellationToken)
        ?? throw new NotFoundException(_t["Service {0} Not Found.", request.Id]);
}