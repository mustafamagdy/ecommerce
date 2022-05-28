using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace FSH.WebApi.Application.Multitenancy;

public class TenantSequenceGenerator
{
  private static readonly object Lock = new object();
  private readonly string _sequenceFilesDir;

  public TenantSequenceGenerator(IConfiguration config)
  {
    string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    _sequenceFilesDir = Path.Combine(path, config["DatabaseSettings:SequenceDir"]);
    if (!Directory.Exists(_sequenceFilesDir))
    {
      Directory.CreateDirectory(_sequenceFilesDir);
    }
  }

  public long Next(string entityName)
  {
    string key = "test";
    string tenantSequencesFile = Path.Combine(_sequenceFilesDir, key + ".txt");
    lock (Lock)
    {
      using var sFile = File.Open(tenantSequencesFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
      using var sr = new StreamReader(sFile);
      string sequenceValues = sr.ReadToEnd();
      sr.Close();
      sr.Dispose();

      string pattern = $"({entityName})=(?<max>\\d+)";
      var reg = new Regex(pattern);
      var match = reg.Match(sequenceValues);
      long max = 0;
      if (match.Success)
      {
        max = Convert.ToInt64(match.Groups["max"].Value);
      }
      else
      {
        if (sequenceValues.Length != 0)
        {
          sequenceValues += "\n";
        }

        sequenceValues += $"{entityName}=0";
      }

      max++;
      var val = $"$1={max}";
      var newSeq = Regex.Replace(sequenceValues, pattern, val);
      using var sw = new StreamWriter(tenantSequencesFile, append: false);
      sw.Write(newSeq);
      sw.Close();
      return max;
    }
  }
}