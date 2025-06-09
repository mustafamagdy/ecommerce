namespace FSH.WebApi.Domain.HR;

public class LeaveType : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DefaultBalance { get; set; }
}
