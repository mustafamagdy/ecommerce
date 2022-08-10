namespace FSH.WebApi.Domain.Operation;

public class Customer : BaseEntity, IAggregateRoot
{
  public string Name { get; private set; }
  public string PhoneNumber { get; private set; }
  public bool CashDefault { get; private set; }

  public Customer(string name, string phoneNumber, bool cashDefault = false)
  {
    Name = name;
    PhoneNumber = phoneNumber;
    CashDefault = cashDefault;
  }
}