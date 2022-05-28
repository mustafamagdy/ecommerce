namespace FSH.WebApi.Shared.Multitenancy;

public interface ITenantSequenceGenerator
{
  string NextFormatted(string entityName);
  long Next(string entityName);
}