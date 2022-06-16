namespace FSH.WebApi.Application.Common.Persistence;

public interface IDapperDbRepository : ITransientService
{
  Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    where T : IDto;
}