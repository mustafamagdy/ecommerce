using System.Data;
using Dapper;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace FSH.WebApi.Infrastructure.Persistence.Repository;

public class DapperEntityRepository : IDapperEntityRepository
{
  private readonly ApplicationDbContext _dbContext;

  public DapperEntityRepository(ApplicationDbContext dbContext) => _dbContext = dbContext;

  public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity =>
    (await _dbContext.Connection.QueryAsync<T>(sql, param, transaction))
    .AsList();

  public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
  {
    if (_dbContext.Model.GetMultiTenantEntityTypes().All(t => t.ClrType != typeof(T)))
    {
      sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
    }

    var entity = await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

    return entity ?? throw new NotFoundException(string.Empty);
  }

  public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
  {
    if (_dbContext.Model.GetMultiTenantEntityTypes().All(t => t.ClrType != typeof(T)))
    {
      sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
    }

    return _dbContext.Connection.QuerySingleAsync<T>(sql, param, transaction);
  }

  public Task<int> AddAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
  {
    if (_dbContext.Model.GetMultiTenantEntityTypes().All(t => t.ClrType != typeof(T)))
    {
      sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
    }

    return _dbContext.Connection.ExecuteAsync(sql, param, transaction);
  }

  public Task UpdateAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity
  {
    if (_dbContext.Model.GetMultiTenantEntityTypes().All(t => t.ClrType != typeof(T)))
    {
      sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
    }

    return _dbContext.Connection.ExecuteAsync(sql, param, transaction);
  }

  public Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
  {
    return _dbContext.Connection.ExecuteAsync(sql, param, transaction);
  }

  public Task<dynamic> QueryFirstOrDefaultAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
  {
    return _dbContext.Connection.QueryFirstOrDefaultAsync(sql, param, transaction);
  }

  public string DatabaseName => _dbContext.Connection.Database;
}