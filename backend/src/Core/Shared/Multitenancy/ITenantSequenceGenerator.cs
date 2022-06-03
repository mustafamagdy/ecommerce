namespace FSH.WebApi.Shared.Multitenancy;

public interface ITenantSequenceGenerator
{
  Task<string> NextFormatted(string entityName);
  Task<long> Next(string entityName);
}