namespace FSH.WebApi.Application.Operation.Orders;

public class BasicOrderDto : IDto
{
  public Guid Id { get; set; }
  public string OrderNumber { get; set; }
  public DateTime OrderDate { get; set; }
  public decimal Amount { get; set; }
  public decimal Paid { get; set; }
  public decimal Due { get; set; }
}