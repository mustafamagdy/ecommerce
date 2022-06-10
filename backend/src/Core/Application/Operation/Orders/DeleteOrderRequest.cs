namespace FSH.WebApi.Application.Operation.Orders;

public class CancelOrderWithPaymentsRequest : IRequest
{
  public CancelOrderWithPaymentsRequest(Guid id)
  {
    Id = id;
  }

  public Guid Id { get; }
}