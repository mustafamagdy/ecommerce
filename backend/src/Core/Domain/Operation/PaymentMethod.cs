namespace FSH.WebApi.Domain.Operation;

public class PaymentMethod : BaseEntity, IAggregateRoot
{
  public string Name { get; set; }
  public bool CashDefault { get; set; }
}