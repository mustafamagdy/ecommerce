using Microsoft.Extensions.DependencyInjection;

namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

internal sealed class CustomSeederRunner
{
  private readonly ICustomSeeder[] _seeders;

  public CustomSeederRunner(IServiceProvider serviceProvider) =>
    _seeders = serviceProvider.GetServices<ICustomSeeder>()
      .OrderBy(a => a.Order)
      .ToArray();

  public async Task RunSeedersAsync(CancellationToken cancellationToken)
  {
    foreach (var seeder in _seeders)
    {
      await seeder.InitializeAsync(cancellationToken);
    }
  }
}