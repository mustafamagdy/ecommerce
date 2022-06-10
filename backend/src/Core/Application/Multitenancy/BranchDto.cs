namespace FSH.WebApi.Application.Multitenancy;

public class BranchDto : IDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
}