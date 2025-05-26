using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.Customers;

public record DeleteCustomerByIdRequest(Guid Id) : IRequest<Guid>;

public class DeleteCustomerByIdRequestHandler : IRequestHandler<DeleteCustomerByIdRequest, Guid>
{
    private readonly IRepository<Customer> _repository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public DeleteCustomerByIdRequestHandler(IRepository<Customer> repository, IApplicationUnitOfWork uow,
        IStringLocalizer<DeleteCustomerByIdRequestHandler> t)
    {
        _repository = repository;
        _uow = uow;
        _t = t;
    }

    public async Task<Guid> Handle(DeleteCustomerByIdRequest request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);
        _ = customer ?? throw new NotFoundException(_t["Service Catalog Item {0} Not Found.", request.Id]);
        await _repository.DeleteAsync(customer, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return request.Id;
    }
}