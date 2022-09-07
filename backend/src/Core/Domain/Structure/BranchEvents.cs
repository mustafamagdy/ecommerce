namespace FSH.WebApi.Domain.Structure;

public class BranchActivatedEvent : DomainEvent
{
  private readonly Branch _branch;

  public BranchActivatedEvent(Branch branch)
  {
    _branch = branch;
  }
}
public class BranchDeactivatedEvent : DomainEvent
{
  private readonly Branch _branch;

  public BranchDeactivatedEvent(Branch branch)
  {
    _branch = branch;
  }
}