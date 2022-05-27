namespace FSH.WebApi.Domain.Operation;

public class Customer : BaseEntity, IAggregateRoot
{
  public string Name { get; private set; }
  public string PhoneNumber { get; private set; }

  public Customer(string name, string phoneNumber)
  {
    Name = name;
    PhoneNumber = phoneNumber;
  }

  public Customer Update(string? name, string? phoneNumber)
  {
    if (name is not null && Name?.Equals(name) is not true) Name = name;
    if (phoneNumber is not null && PhoneNumber?.Equals(phoneNumber) is not true) PhoneNumber = phoneNumber;
    return this;
  }
}