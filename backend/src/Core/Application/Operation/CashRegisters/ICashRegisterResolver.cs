using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Shared.Finance;

public interface ICashRegisterResolver : ITransientService
{
  Task<Guid> Resolve(object context);
}