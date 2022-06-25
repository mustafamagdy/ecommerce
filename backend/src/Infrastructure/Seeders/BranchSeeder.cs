using System.Reflection;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Domain.Catalog;
using FSH.WebApi.Domain.Structure;
using FSH.WebApi.Infrastructure.Multitenancy;
using FSH.WebApi.Infrastructure.Persistence.Context;
using FSH.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.Extensions.Logging;

namespace FSH.WebApi.Infrastructure.Seeders;

public class BranchSeeder : ICustomSeeder
{
  private readonly ISerializerService _serializerService;
  private readonly ApplicationDbContext _db;
  private readonly ILogger<BranchSeeder> _logger;

  public BranchSeeder(ISerializerService serializerService, ILogger<BranchSeeder> logger, ApplicationDbContext db)
  {
    _serializerService = serializerService;
    _logger = logger;
    _db = db;
  }

  public string Order => "01.01";

  public async Task InitializeAsync(CancellationToken cancellationToken)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (!_db.Branches.Any())
    {
      _logger.LogInformation("Started to Seed Branches");

      string jsonData = await File.ReadAllTextAsync(path + "/Seeders/branch.json", cancellationToken);
      var items = _serializerService.Deserialize<List<Branch>>(jsonData);

      foreach (var item in items)
      {
        item.Id = Guid.NewGuid();
        await _db.Branches.AddAsync(item, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Seeded Branches");
    }
  }
}