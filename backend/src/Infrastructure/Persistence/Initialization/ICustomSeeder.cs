namespace FSH.WebApi.Infrastructure.Persistence.Initialization;

public interface ICustomSeeder
{
  string Order { get; }
  Task InitializeAsync(CancellationToken cancellationToken);
}