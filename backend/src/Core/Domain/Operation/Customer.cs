namespace FSH.WebApi.Domain.Operation;

public class Customer : BaseEntity, IAggregateRoot
{
  private readonly List<Order> _orders = new();
  public string Name { get; private set; }
  public string PhoneNumber { get; private set; }
  public bool CashDefault { get; private set; }

  public Customer(string name, string phoneNumber, bool cashDefault = false)
  {
    Name = name;
    PhoneNumber = phoneNumber;
    CashDefault = cashDefault;
  }

  public decimal TotalOrders { get; private set; }
  public decimal DueAmount { get; private set; }

  public void AddOrder(Order order)
  {
    _orders.Add(order);
    var netTotal = order.NetAmount;
    var paidAmount = order.OrderPayments?.Sum(a => a.Amount) ?? 0;

    TotalOrders += netTotal;
    var dueAmount = netTotal - paidAmount;
    DueAmount += dueAmount;
  }

  public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

  public void PayDue(decimal amount)
  {
    DueAmount -= amount;
  }
}