using System.Data;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FSH.WebApi.Infrastructure.Persistence.Repository;

public class DapperTenantConnectionAccessor : IDapperTenantConnectionAccessor
{
  private readonly DatabaseFacade _db;

  public DapperTenantConnectionAccessor(TenantDbContext dbContext)
  {
    _db = dbContext.Database;
  }

  public async Task<IDbConnection> GetDbConnection(CancellationToken cancellationToken = default)
  {
    if (!await _db.CanConnectAsync(cancellationToken))
      throw new InvalidOperationException("Unable to connect to database");

    return _db.GetDbConnection();
  }
}