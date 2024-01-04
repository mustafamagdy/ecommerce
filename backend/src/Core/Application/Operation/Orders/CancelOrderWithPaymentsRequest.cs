using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Shared.Persistence;

namespace FSH.WebApi.Application.Operation.Orders;

public class CancelOrderWithPaymentsRequest : IRequest
{
    public CancelOrderWithPaymentsRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}

public class CancelOrderWithPaymentsRequestHandler : IRequestHandler<CancelOrderWithPaymentsRequest>
{
    private readonly IRepositoryWithEvents<Order> _repo;
    private readonly ISystemTime _systemTime;
    private readonly IApplicationUnitOfWork _uow;

    public CancelOrderWithPaymentsRequestHandler(IRepositoryWithEvents<Order> repo, ISystemTime systemTime,
        IApplicationUnitOfWork uow)
    {
        _repo = repo;
        _systemTime = systemTime;
        _uow = uow;
    }

    public async Task Handle(CancelOrderWithPaymentsRequest request, CancellationToken cancellationToken)
    {
        var order = await _repo.FirstOrDefaultAsync(
                        new SingleResultSpecification<Order>()
                            .Query
                            .Include(a => a.OrderPayments)
                            .Include(a => a.OrderItems)
                            .Include(a => a.Customer)
                            .AsSplitQuery()
                            .Where(a => a.Id == request.Id).Specification, cancellationToken)
                    ?? throw new NotFoundException($"Order {request.Id} not found");

        order.AddDomainEvent(new OrderCanceledEvent(order.Id, _systemTime.Now));
        order.Cancel();

        await _uow.CommitAsync(cancellationToken); ;
    }
}