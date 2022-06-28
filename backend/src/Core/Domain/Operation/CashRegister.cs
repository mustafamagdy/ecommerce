using Ardalis.SmartEnum;
using FSH.WebApi.Domain.Structure;

namespace FSH.WebApi.Domain.Operation;

public class CashRegister : BaseEntity, IAggregateRoot
{
  public CashRegister()
  {
  }

  public CashRegister(Guid branchId, string name, string color)
  {
    BranchId = branchId;
    Name = name;
    Color = color;
  }

  public Guid BranchId { get; set; }
  public virtual Branch Branch { get; set; }
  public string Name { get; set; }
  public string Color { get; set; }
  public bool Opened { get; private set; }
  public decimal Balance { get; private set; }

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

  private List<ActivePaymentOperation> _activeOperations = new();
  private List<ArchivedPaymentOperation> _archivedOperations = new();

  public IReadOnlyList<ActivePaymentOperation> ActiveOperations => _activeOperations.AsReadOnly();
  public IReadOnlyList<ArchivedPaymentOperation> ArchivedOperations => _archivedOperations.AsReadOnly();

  public void AddOperation(PaymentOperation operation)
  {
    if (!Opened)
    {
      throw new InvalidOperationException("Cash register is not opened");
    }

    _activeOperations.Add((ActivePaymentOperation)operation);

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
  public virtual CashRegister CashRegister { get; set; }
  public Guid PaymentMethodId { get; set; }
  public virtual PaymentMethod PaymentMethod { get; set; }
}

public class ActivePaymentOperation : PaymentOperation
{
  public static implicit operator ArchivedPaymentOperation(ActivePaymentOperation op)
  {
    return new ArchivedPaymentOperation
    {
      Amount = op.Amount,
      Id = op.Id,
      CashRegisterId = op.CashRegisterId,
      PaymentMethodId = op.PaymentMethodId,
      Type = op.Type,
    };
  }
}

public class ArchivedPaymentOperation : PaymentOperation
{
}

public sealed class PaymentOperationType : SmartEnum<PaymentOperationType>
{
  public static readonly PaymentOperationType In = new(nameof(In), 1);
  public static readonly PaymentOperationType PendingIn = new(nameof(PendingIn), 2);
  public static readonly PaymentOperationType Out = new(nameof(Out), 3);
  public static readonly PaymentOperationType PendingOut = new(nameof(PendingOut), 4);

  public PaymentOperationType(string name, int value)
    : base(name, value)
  {
  }
}