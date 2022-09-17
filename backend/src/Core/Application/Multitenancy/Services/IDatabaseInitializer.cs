using FSH.WebApi.Domain.MultiTenancy;

namespace FSH.WebApi.Application.Multitenancy.Services;

public interface IDatabaseInitializer
{
  Task InitializeDatabasesAsync(CancellationToken cancellationToken);
  Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken);
}