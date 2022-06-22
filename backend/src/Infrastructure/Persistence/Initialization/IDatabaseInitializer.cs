using FSH.WebApi.Domain.MultiTenancy;
using FSH.WebApi.Infrastructure.Multitenancy;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(FSHTenantInfo tenant, CancellationToken cancellationToken);
}