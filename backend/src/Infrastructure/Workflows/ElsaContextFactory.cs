using Elsa.Persistence.EntityFramework.Core;
using FSH.WebApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FSH.WebApi.Infrastructure.Workflows;

public class ElsaContextFactory : IDesignTimeDbContextFactory<ElsaContext>
{
  public ElsaContext CreateDbContext(string[] args)
  {
    var config = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddCommandLine(args)
      .Build();

    var dbContextBuilder = new DbContextOptionsBuilder();
    var db = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
    dbContextBuilder.UseDatabaseForElsa(db.DBProvider, db.ConnectionString);
    return new ElsaContext(dbContextBuilder.Options);
  }
}