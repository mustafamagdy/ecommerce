using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Operation.CashRegisters;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FSH.WebApi.Infrastructure.Finance;

public class CashRegisterService : ICashRegisterService
{
  private readonly ApplicationDbContext _db;

  public CashRegisterService(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task RegisterPayment(ActivePaymentOperation payment)
  {
    var paymentMethod = await _db.PaymentMethods.FindAsync(payment.PaymentMethodId);
    if (paymentMethod == null)
    {
      throw new NotFoundException($"Payment method {payment.PaymentMethodId} not found.");
    }

    payment.CashRegister.AddOperation(payment);
    _db.Entry(payment).State = EntityState.Added;
  }

  public async Task RegisterPayment(CashRegister cashRegister, Guid paymentMethodId, decimal amount, DateTime date,
    PaymentOperationType type) => RegisterPayment(new ActivePaymentOperation(cashRegister, paymentMethodId, amount, date, type));
}