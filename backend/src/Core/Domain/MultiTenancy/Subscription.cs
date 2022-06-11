namespace FSH.WebApi.Domain.MultiTenancy;

public class Subscription : BaseEntity
{
  public bool DefaultMonthly { get; set; }
  public string Name { get; set; }
  public int Days { get; set; }
  public decimal MonthlyPrice { get; set; }
  public decimal? YearlyPrice { get; set; }
}