namespace FSH.WebApi.Domain.Operation;

public class Customer : BaseEntity, IAggregateRoot
{
  public string Name { get; private set; }
  public string PhoneNumber { get; private set; }
  public bool CashDefault { get; private set; }

  public Customer(string name, string phoneNumber, bool cashDefault)
  {
    Name = name;
    PhoneNumber = phoneNumber;
    CashDefault = cashDefault;
  }

  public Customer Update(string? name, string? phoneNumber, bool? cashDefault)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (phoneNumber is not null && PhoneNumber?.Equals(phoneNumber) is not true) PhoneNumber = phoneNumber;
    if (cashDefault is not null && CashDefault != cashDefault) CashDefault = cashDefault.Value;
    return this;
  }
}