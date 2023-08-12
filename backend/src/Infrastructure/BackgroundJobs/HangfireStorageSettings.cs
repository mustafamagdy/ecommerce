namespace FSH.WebApi.Infrastructure.BackgroundJobs;

public sealed class HangfireStorageSettings
{
  public string? StorageProvider { get; set; }
  public string? ConnectionString { get; set; }
}