using FSH.WebApi.Application.Common.Caching;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Caching;

public sealed class LocalCacheService : ICacheService
{
  private readonly ILogger<LocalCacheService> _logger;
  private readonly IAppCache _cache;

  public LocalCacheService(IAppCache cache, ILogger<LocalCacheService> logger) =>
    (_cache, _logger) = (cache, logger);

  public T? Get<T>(string key) =>
    _cache.Get<T>(key);

  public Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>
    Task.FromResult(Get<T>(key));

  public void Refresh(string key) =>
    _cache.TryGetValue<dynamic>(key, out _);

  public Task RefreshAsync(string key, CancellationToken token = default)
  {
    Refresh(key);
    return Task.CompletedTask;
  }

  public void Remove(string key) =>
    _cache.Remove(key);

  public Task RemoveAsync(string key, CancellationToken token = default)
  {
    Remove(key);
    return Task.CompletedTask;
  }

  public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null)
  {
    if (slidingExpiration is null)
    {
      // TODO: add to appsettings?
      slidingExpiration = TimeSpan.FromMinutes(10); // Default expiration time of 10 minutes.
    }

    _cache.Add(key, value, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
    _logger.LogDebug($"Added to Cache : {key}", key);
  }

  public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken token = default)
  {
    Set(key, value, slidingExpiration);
    return Task.CompletedTask;
  }
}