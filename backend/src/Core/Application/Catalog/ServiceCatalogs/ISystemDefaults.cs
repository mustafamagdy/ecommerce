namespace FSH.WebApi.Application.Catalog.ServiceCatalogs;

public interface ISystemDefaults : ITransientService
{
  Task<Brand?> GetDefaultBrandAsync(CancellationToken cancellationToken = default);
}

public class SystemDefaults : ISystemDefaults
{
  private readonly IReadRepository<Brand> _brandRepo;

  public SystemDefaults(IReadRepository<Brand> brandRepo)
  {
    _brandRepo = brandRepo;
  }

  public Task<Brand?> GetDefaultBrandAsync(CancellationToken cancellationToken = default) =>
    _brandRepo.FirstOrDefaultAsync(new SingleResultSpecification<Brand>().Query.Where(a => a.SystemDefault).Specification, cancellationToken)
    ?? throw new InvalidOperationException("No default brand configured");
}