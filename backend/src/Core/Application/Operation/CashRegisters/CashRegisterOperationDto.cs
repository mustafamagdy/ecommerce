namespace FSH.WebApi.Application.Operation.CashRegisters;

public abstract class CashRegisterOperationDto : IDto
{
  public DateTime DateTime { get; set; }
  public decimal Amount { get; set; }
  public string PaymentOperationType { get; set; }
  public virtual string PaymentMethodName { get; set; }
}