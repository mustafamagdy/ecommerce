namespace FSH.WebApi.Application.Multitenancy.Services;

public interface ITenantConnectionStringBuilder
{
  string BuildConnectionString(string databaseName);
}