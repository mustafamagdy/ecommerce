using System.Text.Json;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Multitenancy;
using FSH.WebApi.Application.Multitenancy.Services;
using FSH.WebApi.Infrastructure.Common.Extensions;
using FSH.WebApi.Infrastructure.Persistence;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Infrastructure.Multitenancy
{
  public class TenantConnectionStringBuilder : ITenantConnectionStringBuilder
  {
    private readonly IConfiguration _config;
    private readonly IConnectionStringValidator _csValidator;
    private readonly IHostEnvironment _env;

    public TenantConnectionStringBuilder(IConfiguration config, IConnectionStringValidator csValidator, IHostEnvironment env)
    {
      _config = config;
      _csValidator = csValidator;
      _env = env;
    }

    public string BuildConnectionString(string tenantId, SubscriptionType subscriptionType)
    {
      string connectionStringTemplate = _config["DatabaseSettings:ConnectionStringTemplate"];
      string providerName = _config["DatabaseSettings:DBProvider"];

      string dbName = $"{_env.GetShortenName()}-{tenantId}-{subscriptionType}";
      string connectionString = string.Format(connectionStringTemplate, dbName);
      if (!_csValidator.TryValidate(connectionString, providerName))
        throw new ArgumentException(connectionString);

      return connectionString;
    }
  }
}