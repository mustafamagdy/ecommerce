using System.Data;
using Dapper;
using Finbuckle.MultiTenant;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Infrastructure.Common.Extensions;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace FSH.WebApi.Application.Multitenancy;

public class TableExist
{
  public int tableCount { get; set; }
}

public class NpgsqlTenantSequenceGenerator : ITenantSequenceGenerator
{
  private readonly ITenantInfo _currentTenant;
  private readonly IHostEnvironment _env;
  private readonly IDapperEntityRepository _counterRepo;
  private readonly string _sequenceTableName;
  private bool? _counterTableExist;
  private readonly string? _connectionString;

  public NpgsqlTenantSequenceGenerator(IConfiguration config, ITenantInfo currentTenant, IHostEnvironment env, IDapperEntityRepository counterRepo)
  {
    _currentTenant = currentTenant;
    _env = env;
    _counterRepo = counterRepo;
    _sequenceTableName = config["DatabaseSettings:SequenceTableName"];
    _connectionString = string.Format(config["DatabaseSettings:ConnectionStringTemplate"], _counterRepo.DatabaseName);
  }

  private async Task CheckCounterTableExist(string tableName)
  {
    // var sql = @"SELECT count(*) AS tableCount FROM information_schema.TABLES
    //              WHERE (TABLE_SCHEMA = @dbName) AND  (TABLE_NAME = @tableName);";

    var sql = @"SELECT cast(count(*) as int) AS tableCount FROM information_schema.TABLES
                 WHERE (table_catalog = @dbName) AND  (TABLE_NAME = @tableName);";
    var result = await _counterRepo.QueryFirstOrDefaultNoneEntAsync<TableExist>(sql, new { dbName = _counterRepo.DatabaseName, tableName });
    _counterTableExist = result.tableCount > 0;

    if (!_counterTableExist.Value)
    {
      sql = $"CREATE TABLE {Escape(tableName)} (\"entityName\" VARCHAR(100) NOT NULL, \"current\" BIGINT NOT NULL "
            + $"DEFAULT 0, CONSTRAINT \"PK_{tableName}\" PRIMARY KEY (\"entityName\"));";
      await _counterRepo.ExecuteAsync(sql);
    }
  }

  public async Task<string> NextFormatted(string entityName)
  {
    var next = await Next(entityName);
    return next.ToString().PadLeft(7, '0');
  }

  private string Escape(string str)
  {
    return $"\"{str}\"";
  }

  public async Task<long> Next(string entityName)
  {
    if (_counterTableExist == null)
    {
      await CheckCounterTableExist(_sequenceTableName);
    }

    long next = 0;
    string sequenceName = $"{_env.GetShortenName()}-{_currentTenant.Identifier}-{entityName}";

    IDbTransaction trx = null;
    NpgsqlConnection cnn = null;
    try
    {
      cnn = new NpgsqlConnection(_connectionString);
      cnn.Open();
      trx = await cnn.BeginTransactionAsync();

      string sql = $"SELECT \"current\" from {Escape(_sequenceTableName)}  where \"entityName\" = @entityName"
                   + " for update;";
      dynamic result = await cnn.QueryFirstOrDefaultAsync(sql, new { entityName = sequenceName }, trx);

      int i;
      if (result == null)
      {
        next = 1;
        sql = $"insert into {Escape(_sequenceTableName)} (\"entityName\", \"current\") values(@entityName, @next);";
        i = await cnn.ExecuteAsync(sql, new { next, entityName = sequenceName }, trx);
      }
      else
      {
        next = Convert.ToInt64(result.current);
        next++;
        sql = $"update {Escape(_sequenceTableName)} set \"current\" = @next where \"entityName\" = @entityName";
        i = await cnn.ExecuteAsync(sql, new { next, entityName = sequenceName }, trx);
      }

      if (i != 1)
      {
        trx.Rollback();
        throw new InvalidOperationException("Sequence generation failed for entity " + sequenceName);
      }

      trx.Commit();
    }
    catch (Exception ex)
    {
      trx?.Rollback();
    } finally
    {
      await cnn?.CloseAsync()!;
    }

    return next;
  }
}