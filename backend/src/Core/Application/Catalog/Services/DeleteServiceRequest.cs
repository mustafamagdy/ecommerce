using FSH.WebApi.Domain.Common.Events;

namespace FSH.WebApi.Application.Catalog.Services;

public class DeleteServiceRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteServiceRequest(Guid id) => Id = id;
}

public class DeleteServiceRequestHandler : IRequestHandler<DeleteServiceRequest, Guid>
{
    private readonly IRepository<Service> _repository;
    private readonly IStringLocalizer _t;

    public DeleteServiceRequestHandler(IRepository<Service> repository, IStringLocalizer<DeleteServiceRequestHandler>
    localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<Guid> Handle(DeleteServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetByIdAsync(request.Id, cancellationToken);

        _ = service ?? throw new NotFoundException(_t["Service {0} Not Found."]);

        // Add Domain Events to be raised after the commit
        service.DomainEvents.Add(EntityDeletedEvent.WithEntity(service));

        await _repository.DeleteAsync(service, cancellationToken);

        return request.Id;
    }
}