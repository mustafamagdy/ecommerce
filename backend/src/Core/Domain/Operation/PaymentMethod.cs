namespace FSH.WebApi.Domain.Operation;

public class PaymentMethod : BaseEntity, IAggregateRoot
{
  public PaymentMethod(string name, bool cashDefault)
  {
    Name = name;
    CashDefault = cashDefault;
  }

  public string Name { get; private set; }
  public bool CashDefault { get; private set; }
}