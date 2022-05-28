using System.Reflection;
using System.Text.RegularExpressions;
using Finbuckle.MultiTenant;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FSH.WebApi.Application.Multitenancy;

public class TenantSequenceGenerator : ITenantSequenceGenerator
{
  private readonly ITenantInfo _currentTenant;
  private readonly IHostEnvironment _env;
  private static readonly object Lock = new object();
  private readonly string _sequenceFilesDir;

  public TenantSequenceGenerator(IConfiguration config, ITenantInfo currentTenant, IHostEnvironment env)
  {
    _currentTenant = currentTenant;
    _env = env;
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    _sequenceFilesDir = Path.Combine(path, config["DatabaseSettings:SequenceDir"]);
    if (!Directory.Exists(_sequenceFilesDir))
    {
      Directory.CreateDirectory(_sequenceFilesDir);
    }
  }

  public string NextFormatted(string entityName)
  {
    var next = Next(entityName);
    return next.ToString().PadLeft(7, '0');
  }

  public long Next(string entityName)
  {
    string sequenceFileName = $"{_env.GetShortenName()}-{_currentTenant.Identifier}.txt";
    string tenantSequencesFile = Path.Combine(_sequenceFilesDir, sequenceFileName);
    lock (Lock)
    {
      using var sFile = File.Open(tenantSequencesFile, FileMode.OpenOrCreate, FileAccess.Read);
      using var sr = new StreamReader(sFile);
      string sequenceValues = sr.ReadToEnd();
      sr.Close();
      sr.Dispose();

      string pattern = $"({entityName})=(?<max>\\d+)";
      var regex = new Regex(pattern);
      var match = regex.Match(sequenceValues);
      long maxCount = 0;
      if (match.Success)
      {
        if (!long.TryParse(match.Groups["max"].Value, out maxCount))
          maxCount = 0;
      }
      else
      {
        if (sequenceValues.Length != 0)
        {
          sequenceValues += "\n";
        }

        sequenceValues += $"{entityName}=0";
      }

      maxCount++;
      string replacePattern = $"$1={maxCount}";
      string newContent = Regex.Replace(sequenceValues, pattern, replacePattern);
      using var sw = new StreamWriter(tenantSequencesFile, append: false);
      sw.Write(newContent);

      sw.Close();
      sw.Dispose();

      return maxCount;
    }
  }
}