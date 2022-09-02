using Ardalis.SmartEnum;
using FSH.WebApi.Domain.Structure;

namespace FSH.WebApi.Domain.Operation;

public class CashRegister : AuditableEntity, IAggregateRoot
{
  private CashRegister()
  {
  }

  public CashRegister(Guid managerId, Guid branchId, string name, string color)
  {
    ManagerId = managerId;
    BranchId = branchId;
    Name = name;
    Color = color;
  }

  private List<ActivePaymentOperation> _activeOperations = new();
  private readonly List<ArchivedPaymentOperation> _archivedOperations = new();

  public string Name { get; set; }
  public string Color { get; set; }
  public bool Opened { get; private set; }
  public decimal Balance { get; private set; }

  public Guid ManagerId { get; set; }
  public Guid BranchId { get; set; }
  public Branch Branch { get; private set; }

  public IReadOnlyCollection<ActivePaymentOperation> ActiveOperations => _activeOperations.AsReadOnly();
  public IReadOnlyCollection<ArchivedPaymentOperation> ArchivedOperations => _archivedOperations.AsReadOnly();

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
      .Where(a => a.OperationType != PaymentOperationType.PendingIn
                  || a.OperationType != PaymentOperationType.PendingOut)
      .ToList();

    _archivedOperations.AddRange(committedOperations.Select(ArchivedPaymentOperation.From));

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
    if (operation.OperationType != PaymentOperationType.PendingIn)
    {
      throw new InvalidOperationException($"Operation {operation.OperationType} is not valid for accept pending in operation");
    }

    operation.OperationType = PaymentOperationType.In;
    UpdateBalance(operation);
  }

  public void CommitPendingOut(PaymentOperation operation)
  {
    if (operation.OperationType != PaymentOperationType.PendingOut)
    {
      throw new InvalidOperationException($"Operation {operation.OperationType} is not valid for committing pending out operation");
    }

    operation.OperationType = PaymentOperationType.Out;
  }

  private void UpdateBalance(PaymentOperation operation)
  {
    switch (operation.OperationType.Name)
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

public abstract class PaymentOperation : AuditableEntity, IAggregateRoot
{
  public DateTime DateTime { get; set; }
  public decimal Amount { get; set; }
  public PaymentOperationType OperationType { get; set; }

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
    DateTime date, PaymentOperationType operationType)
  {
    Amount = amount;
    OperationType = operationType;
    DateTime = date;
    PaymentMethodId = paymentMethodId;
    CashRegister = cashRegister;
    CashRegisterId = cashRegister.Id;
  }

  public static (ActivePaymentOperation Source, ActivePaymentOperation Destination) CreateTransfer(Guid sourceCashRegisterId,
    Guid destinationCashRegisterId, decimal amount, DateTime opDate, Guid transferPaymentMethodId)
  {
    var src = new ActivePaymentOperation
    {
      Id = Guid.NewGuid(),
      Amount = amount,
      OperationType = PaymentOperationType.PendingOut,
      DateTime = opDate,
      CashRegisterId = sourceCashRegisterId,
      PaymentMethodId = transferPaymentMethodId
    };

    var dest = new ActivePaymentOperation
    {
      Id = Guid.NewGuid(),
      Amount = amount,
      OperationType = PaymentOperationType.PendingIn,
      DateTime = opDate,
      CashRegisterId = destinationCashRegisterId,
      PendingTransferId = src.Id,
      PaymentMethodId = transferPaymentMethodId
    };

    return (src, dest);
  }
}

public class ArchivedPaymentOperation : PaymentOperation
{
  public static ArchivedPaymentOperation From(ActivePaymentOperation activeOp)
  {
    return new ArchivedPaymentOperation
    {
      Amount = activeOp.Amount,
      CashRegisterId = activeOp.CashRegisterId,
      OperationType = activeOp.OperationType,
      PaymentMethodId = activeOp.PaymentMethodId,
      PendingTransferId = activeOp.PendingTransferId,
    };
  }
}

public sealed class PaymentOperationType : SmartEnum<PaymentOperationType, string>
{
  public static readonly PaymentOperationType In = new(nameof(In), "in");
  public static readonly PaymentOperationType PendingIn = new(nameof(PendingIn), "pending-in");
  public static readonly PaymentOperationType Out = new(nameof(Out), "out");
  public static readonly PaymentOperationType PendingOut = new(nameof(PendingOut), "pending-out");

  private PaymentOperationType(string name, string value)
    : base(name, value)
  {
  }
}