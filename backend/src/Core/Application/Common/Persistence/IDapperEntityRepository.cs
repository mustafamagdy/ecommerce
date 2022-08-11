using System.Data;

namespace FSH.WebApi.Application.Common.Persistence;

public interface IDapperEntityRepository : ITransientService
{
  Task<IReadOnlyCollection<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity;

  Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity;

  Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity;

  Task<int> AddAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity;

  Task UpdateAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    where T : class, IEntity;

  Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

  Task<dynamic> QueryFirstOrDefaultAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

  string DatabaseName { get; }
}