using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Shared.Finance;

public interface ICashRegisterResolver : ITransientService
{
  Task<CashRegister> Resolve(object context);
}