using System.Reflection;
using System.Text.RegularExpressions;
using FSH.WebApi.Infrastructure.Multitenancy;
using Microsoft.Extensions.Configuration;

namespace FSH.WebApi.Infrastructure.Persistence;

public class TenantSequenceGenerator
{
  private readonly FSHTenantInfo _currentTenant;
  private static readonly object Lock = new object();
  private readonly string _sequenceFilesDir;

  public TenantSequenceGenerator(FSHTenantInfo currentTenant, IConfiguration config)
  {
    _currentTenant = currentTenant;
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    _sequenceFilesDir = Path.Combine(path, config["DatabaseSettings:SequenceDir"]);
    if (!Directory.Exists(_sequenceFilesDir))
    {
      Directory.CreateDirectory(_sequenceFilesDir);
    }
  }

  public long Next(string entityName)
  {
    var tenantSequencesFile = Path.Combine(_sequenceFilesDir, _currentTenant.Key.ToLower(), ".txt");
    lock (Lock)
    {
      using var sFile = File.Open(tenantSequencesFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
      using var sr = new StreamReader(sFile);
      string sequenceValues = sr.ReadToEnd();
      var reg = new Regex("^orderitems=(?<max>\\d+)");
      var match = reg.Match(sequenceValues);
      long max = 0;
      if (match.Success)
      {
        max = Convert.ToInt64(match.Groups["max"].Value);
      }

      max++;
      var newSeq = reg.Replace(sequenceValues, max.ToString());
      // var seqValues = sequenceValues.Split('\n').Select(a => a.Split('=')).FirstOrDefault(a => a[0] == entityName);
      // long lastVal = seqValues == null ? 0 : Convert.ToInt64(seqValues[1]);
      // lastVal++;
      //
      // sr.Close();
      // using var sw = new StreamWriter(sFile);

      return 0;
    }
  }
}