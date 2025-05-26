namespace FSH.WebApi.Domain.Operation;

public sealed class PaymentMethod : BaseEntity, IAggregateRoot
{
  private PaymentMethod()
  {
  }

  public PaymentMethod(string name, bool cashDefault)
  {
    Name = name;
    CashDefault = cashDefault;
  }

  public string Name { get; private set; }
  public bool CashDefault { get; private set; }
}