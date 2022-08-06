using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.CashRegisters;

public interface ICashRegisterService : ITransientService
{
  Task RegisterPayment(ActivePaymentOperation payment);
  Task RegisterPayment(CashRegister cashRegister, Guid paymentMethodId, decimal amount, DateTime date, PaymentOperationType type);
}