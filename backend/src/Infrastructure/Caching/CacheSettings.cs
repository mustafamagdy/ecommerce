namespace FSH.WebApi.Infrastructure.Caching;

public sealed class CacheSettings
{
  public bool UseDistributedCache { get; set; }
  public bool PreferRedis { get; set; }
  public string? RedisURL { get; set; }
}