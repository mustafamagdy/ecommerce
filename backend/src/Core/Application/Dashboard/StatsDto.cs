namespace FSH.WebApi.Application.Dashboard;

public class StatsDto : IDto
{
  public int ServiceCatalogCount { get; set; }
  public int OrderCount { get; set; }
  public int BranchCount { get; set; }
  public int UserCount { get; set; }
  public int RoleCount { get; set; }
  public List<ChartSeries> DataEnterBarChart { get; set; } = new();
  public Dictionary<string, double>? OrdersByBranchPieChart { get; set; } = new();
}

public class ChartSeries
{
  public string? Name { get; set; }
  public double[]? Data { get; set; }
}