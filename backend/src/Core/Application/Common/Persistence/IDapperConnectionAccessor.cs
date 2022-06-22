using System.Data;

namespace FSH.WebApi.Application.Common.Persistence;

public interface IDapperConnectionAccessor
{
  Task<IDbConnection> GetDbConnection(CancellationToken cancellationToken = default);
}

public interface IDapperAppConnectionAccessor : IDapperConnectionAccessor, ITransientService
{
}

public interface IDapperTenantConnectionAccessor : IDapperConnectionAccessor, ITransientService
{
}