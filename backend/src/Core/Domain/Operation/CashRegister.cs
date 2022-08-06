using Ardalis.SmartEnum;
using FSH.WebApi.Domain.Structure;

namespace FSH.WebApi.Domain.Operation;

public class CashRegister : BaseEntity, IAggregateRoot
{
  private CashRegister()
  {
  }

  public CashRegister(Guid branchId, string name, string color)
  {
    BranchId = branchId;
    Name = name;
    Color = color;
  }

  private List<ActivePaymentOperation> _activeOperations = new();
  private readonly List<ArchivedPaymentOperation> _archivedOperations = new();

  public Guid BranchId { get; set; }
  public virtual Branch Branch { get; set; }
  public string Name { get; set; }
  public string Color { get; set; }
  public bool Opened { get; private set; }
  public decimal Balance { get; private set; }

  public virtual IReadOnlyList<ActivePaymentOperation> ActiveOperations => _activeOperations.AsReadOnly();
  public virtual IReadOnlyList<ArchivedPaymentOperation> ArchivedOperations => _archivedOperations.AsReadOnly();

  public void Open()
  {
    Opened = true;
  }

  public void Close()
  {
    MoveAllOperationsToArchive();
    Opened = false;
  }

  private void MoveAllOperationsToArchive()
  {
    var committedOperations = _activeOperations
      .Where(a => a.Type != PaymentOperationType.PendingIn
                  || a.Type != PaymentOperationType.PendingOut)
      .ToList();

    _archivedOperations.AddRange(committedOperations.Cast<ArchivedPaymentOperation>());

    _activeOperations = _activeOperations.Except(committedOperations).ToList();
  }

  public void AddOperation(ActivePaymentOperation operation)
  {
    if (!Opened)
    {
      throw new InvalidOperationException("Cash register is not opened");
    }

    _activeOperations.Add(operation);

    UpdateBalance(operation);
  }

  public void AcceptPendingIn(PaymentOperation operation)
  {
    if (operation.Type != PaymentOperationType.PendingIn)
    {
      throw new InvalidOperationException($"Operation {operation.Type} is not valid for accept pending in operation");
    }

    operation.Type = PaymentOperationType.In;
    UpdateBalance(operation);
  }

  public void CommitPendingOut(PaymentOperation operation)
  {
    if (operation.Type != PaymentOperationType.PendingOut)
    {
      throw new InvalidOperationException($"Operation {operation.Type} is not valid for committing pending out operation");
    }

    operation.Type = PaymentOperationType.Out;
  }

  private void UpdateBalance(PaymentOperation operation)
  {
    switch (operation.Type.Name)
    {
      case nameof(PaymentOperationType.In):
        Balance += operation.Amount;
        break;
      case nameof(PaymentOperationType.Out):
      case nameof(PaymentOperationType.PendingOut):
        Balance -= operation.Amount;
        break;
    }
  }
}

public abstract class PaymentOperation : BaseEntity, IAggregateRoot
{
  public DateTime DateTime { get; set; }
  public decimal Amount { get; set; }
  public PaymentOperationType Type { get; set; }

  public Guid CashRegisterId { get; set; }
  public CashRegister CashRegister { get; set; }
  public Guid PaymentMethodId { get; set; }
  public PaymentMethod PaymentMethod { get; set; }

  public Guid? PendingTransferId { get; set; }
}

public class ActivePaymentOperation : PaymentOperation
{
  private ActivePaymentOperation()
  {
  }

  public ActivePaymentOperation(CashRegister cashRegister, Guid paymentMethodId, decimal amount,
    DateTime date, PaymentOperationType type)
  {
    Amount = amount;
    Type = type;
    DateTime = date;
    PaymentMethodId = paymentMethodId;
    CashRegister = cashRegister;
    CashRegisterId = cashRegister.Id;
  }

  public static (ActivePaymentOperation source, ActivePaymentOperation dest) CreateTransfer(Guid sourceCashRegisterId,
    Guid destinationCashRegisterId, decimal amount, DateTime opDate)
  {
    var src = new ActivePaymentOperation
    {
      Id = Guid.NewGuid(),
      Amount = amount,
      Type = PaymentOperationType.PendingOut,
      DateTime = opDate,
      CashRegisterId = sourceCashRegisterId
    };

    var dest = new ActivePaymentOperation
    {
      Id = Guid.NewGuid(),
      Amount = amount,
      Type = PaymentOperationType.PendingIn,
      DateTime = opDate,
      CashRegisterId = destinationCashRegisterId,
      PendingTransferId = src.Id
    };

    return (src, dest);
  }
}

public class ArchivedPaymentOperation : PaymentOperation
{
}

public sealed class PaymentOperationType : SmartEnum<PaymentOperationType, string>
{
  public static readonly PaymentOperationType In = new(nameof(In), "in");
  public static readonly PaymentOperationType PendingIn = new(nameof(PendingIn), "pending-in");
  public static readonly PaymentOperationType Out = new(nameof(Out), "out");
  public static readonly PaymentOperationType PendingOut = new(nameof(PendingOut), "pending-out");

  public PaymentOperationType()
    : base(null, null)
  {
  }

  private PaymentOperationType(string name, string value)
    : base(name, value)
  {
  }
}