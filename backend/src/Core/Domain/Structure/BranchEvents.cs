namespace FSH.WebApi.Domain.Structure;

public sealed class BranchActivatedEvent : DomainEvent
{
  private readonly Branch _branch;

  public BranchActivatedEvent(Branch branch)
  {
    _branch = branch;
  }
}

public sealed class BranchDeactivatedEvent : DomainEvent
{
  private readonly Branch _branch;

  public BranchDeactivatedEvent(Branch branch)
  {
    _branch = branch;
  }
}